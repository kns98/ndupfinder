using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace deduper.wpf
{
    public class DuplicateGroupCollection :
        ObservableCollection<DuplicateGroup>
    {
        private readonly Dictionary<string, DuplicateGroup> hashlookup =
            new Dictionary<string, DuplicateGroup>();

        //inject using inheritance
        //easiest way to use constructor injection

        public IBitMapCreator Bc { get; set; }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        public async Task Update(IDirectory root)
        {
            var dff = SetupDff(null, root);
            Running = true;
            await dff.FindDuplicates();
            Running = false;
        }


        public Task UpdateAsync(IDispatcher dispatcher, IDirectory root)
        {
            var dff = SetupDff(dispatcher, root);

            Action start = () => Running = true;
            Action stop = () => Running = false;

            return Task.Run(() =>
            {
                if (dispatcher != null)
                    dispatcher.Execute(start);
                else
                    start();

                Task.WaitAll(dff.FindDuplicates());

                if (dispatcher != null)
                    dispatcher.Execute(stop);
                else
                    stop();
            });
        }

        private DuplicateFileFinder SetupDff(IDispatcher dispatcher, IDirectory root)
        {
            Clear();
            hashlookup.Clear();

            var dff = new DuplicateFileFinder(
                dispatcher,
                root);

            dff.OnDuplicateFound += dff_OnDuplicateFound;
            dff.OnFileScanned += dff_OnFileScanned;
            dff.OnHashProgress += dff_OnHashProgress;
            return dff;
        }

        #region event handlers

        protected void dff_OnFileScanned(object sender, EventArgs e)
        {
            CurrentAction = "Scanning File " + (sender as IFile).Path;
        }

        protected void dff_OnHashProgress(string filepath, float percent_done)
        {
            if (percent_done == 0.0)
            {
                CurrentAction = "Creating MD5 Hash for " + filepath;
                Blink = true;
            }

            if (percent_done == 100.0)
                //CurrentAction = "Completed MD5 Hash for " + filepath;
                Blink = false;
        }

        //public  StringBuilder designTimeDataCodeBuilder = new StringBuilder();

        protected void dff_OnDuplicateFound(string hashcode, string filepath, long size)
        {
            /*
            designTimeDataCodeBuilder.Append("dff_OnDuplicateFound(");

            designTimeDataCodeBuilder.Append("\"");
            designTimeDataCodeBuilder.Append(hashcode);
            designTimeDataCodeBuilder.Append("\",");

            designTimeDataCodeBuilder.Append("@\"");
            designTimeDataCodeBuilder.Append(filepath);
            designTimeDataCodeBuilder.Append("\",");

            designTimeDataCodeBuilder.Append(size);
            designTimeDataCodeBuilder.Append(");\n");
            */

            DuplicateGroup c;

            if (!hashlookup.TryGetValue(hashcode, out c))
            {
                c = new DuplicateGroup(Bc);
                hashlookup.Add(hashcode, c);
                Add(c);
            }

            c.Add(new Duplicate(filepath, size));
        }

        #endregion

        #region running

        private bool _running;

        public bool Running
        {
            get => _running;
            set
            {
                if (_running != value) //don't fire event if the value is unchanged
                {
                    _running = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion


        #region blink

        private bool _blink;

        public bool Blink
        {
            get => _blink;
            set
            {
                if (_blink != value) //don't fire event if the value is unchanged
                {
                    _blink = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion

        #region currentaction

        private string _currentaction;

        public string CurrentAction
        {
            get => _currentaction;
            set
            {
                _currentaction = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}