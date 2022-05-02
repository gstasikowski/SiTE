using SiTE.Helpers;
using SiTE.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace SiTE.Models
{
    public class ApplicationViewModel : ObservableObject
    {
        #region Fields
        private ICommand _changePageCommand;
        private IPageViewModel _currentPageViewModel;
        private List<IPageViewModel> _pageViewModels;

        private bool _settingsModified;
        #endregion

        public ApplicationViewModel()
        {
            PageViewModels.Add(new Views.EditorView());
            PageViewModels.Add(new Views.SettingsView());

            CurrentPageViewModel = PageViewModels[0];
            SettingsModified = false;
        }

        #region Commands & properties
        public ICommand ChangePageCommand
        {
            get
            {
                if (_changePageCommand == null)
                {
                    _changePageCommand = new RelayCommand(p => ChangeViewModel((IPageViewModel)p), p => p is IPageViewModel);
                }

                return _changePageCommand;
            }
        }

        public List<IPageViewModel> PageViewModels
        {
            get
            {
                if (_pageViewModels == null)
                    _pageViewModels = new List<IPageViewModel>();

                return _pageViewModels;
            }
        }

        public IPageViewModel CurrentPageViewModel
        {
            get { return _currentPageViewModel; }
            set
            {
                if (_currentPageViewModel != value)
                {
                    _currentPageViewModel = value;
                    OnPropertyChanged("CurrentPageViewModel");
                }
            }
        }

        public bool SettingsModified
        {
            get { return _settingsModified; }
            set
            {
                _settingsModified = value;
                OnPropertyChanged("SettingsModified");
            }
        }
        #endregion Commands & properties

        private void ChangeViewModel(IPageViewModel viewModel)
        {
            if (!PageViewModels.Contains(viewModel))
                PageViewModels.Add(viewModel);

            CurrentPageViewModel = PageViewModels.FirstOrDefault(vm => vm == viewModel);
        }
    }
}
