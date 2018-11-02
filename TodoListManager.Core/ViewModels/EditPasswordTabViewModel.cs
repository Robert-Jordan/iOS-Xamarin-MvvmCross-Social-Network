﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Binding.Combiners;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.Presenters.Hints;
using MvvmCross.ViewModels;
using TodoListManager.Core.Models;
using TodoListManager.Core.Services;
using UIKit;

namespace TodoListManager.Core.ViewModels
{

    public class EditPasswordTabViewModel : MvxViewModel<ProfileTabViewModel>//<IDbService/*,ProfileTabViewModel*/>
    {
        public EditPasswordTabViewModel(IMvxNavigationService navigationService)
        {
            _navigationService = navigationService;
        }

        #region Fields and properties    
        public bool IsNewPasswordValid = false;
        public bool IsOldPasswordValid = false;
        public bool ConfirmValid = false;
        private string _oldPassword;
        private string _newPassword;
        private string _alertMessage;
        private string _confirmNewPassword;
        private UIColor _alertColor;
        private readonly IMvxNavigationService _navigationService;
        private IDbService _dataService;
        private bool _result = false;
        
        public UIColor AlertColor
        {
            get => _alertColor;
            set
            {
                _alertColor = value;
                RaisePropertyChanged();
            }
        }
        public string OldPassword
        {
            get => _oldPassword;
            set
            {
                _oldPassword = value;
                RaisePropertyChanged();
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                RaisePropertyChanged();
            }
        }

        public string ConfirmNewPassword
        {
            get => _confirmNewPassword;
            set
            {
                _confirmNewPassword = value;
                RaisePropertyChanged();
            }
        }

        public string AlertMessage
        {
            get => _alertMessage;
            set
            {
                _alertMessage = value;
                RaisePropertyChanged();
            }

        }
        #endregion

        public ICommand CancelCommand => new MvxCommand(Cancel);

        private void Cancel()
        {
            _navigationService.Close(this);
            _result = false;
            _model.Result += () => _result;
            _model.Begin();
        }

        private ProfileTabViewModel _model;
        public override void Prepare(ProfileTabViewModel dataService)
        {
            _model = dataService;
            _dataService = new DbService();
            //_dataService = dataService;
        }

        #region Validation
        public void CheckOldPassword()
        {
            if (!string.IsNullOrEmpty(_oldPassword))
            {
                var user = _dataService.Query<UserModel>("SELECT * FROM Users WHERE Password = ? AND Login = ?", _oldPassword, HomeViewModel.UserModel.Login).SingleOrDefault();
                if (user == null)
                {
                    AlertColor = UIColor.Red;
                    AlertMessage = "Old password is incorrect!";
                    IsOldPasswordValid = false;
                }
                else
                    IsOldPasswordValid = user.Password == OldPassword;

                if (IsOldPasswordValid)
                {
                    //AlertColor = UIColor.Green;
                    AlertMessage = "";
                }
                else
                {
                    AlertColor = UIColor.Red;
                    AlertMessage = "Old password is incorrect";
                }
            }
            else
            {
                AlertColor = UIColor.Red;
                AlertMessage = "Enter your old password!";
                IsOldPasswordValid = false;
            }
        }
        public void IsPasswordValid()
        {
            if (_newPassword.Length >= 6 && _newPassword == NewPassword && _newPassword.Length <= 36)
            {
                IsNewPasswordValid = true;
                AlertMessage = "";
                //AlertColor = UIColor.Green;
            }
            else
                if (_newPassword.Length < 6)
            {
                IsNewPasswordValid = false;
                AlertColor = UIColor.Red;
                AlertMessage =
                    "Password length must be more than 6 characters.";
            }
            else
                if (_newPassword.Length > 36)
            {
                IsNewPasswordValid = false;
                AlertColor = UIColor.Red;
                AlertMessage =
                    "Password length must be less than 36 characters.";
            }

        }
        public void CheckConfirmPassword()
        {
            if (IsNewPasswordValid)
            {
                if (_newPassword != _confirmNewPassword)
                {
                    ConfirmValid = false;
                    AlertColor = UIColor.Red;
                    AlertMessage = "The password you entered do not match!";
                }
                else
                {
                    ConfirmValid = true;
                    //AlertColor = UIColor.Green;
                    AlertMessage = "";
                }
            }
        }

        public void Validate()
        {
            var _user = _dataService.GetItem<UserModel>(HomeViewModel.UserModel.Id);

            if (!string.IsNullOrEmpty(_newPassword) && IsNewPasswordValid && IsOldPasswordValid && (_newPassword == _confirmNewPassword) && !string.IsNullOrEmpty(_oldPassword) &&
                !string.IsNullOrEmpty(_confirmNewPassword))
            {
                _user.Password = NewPassword;
                HomeViewModel.UserModel.Password = NewPassword;
                AlertMessage = "";
                AlertColor = UIColor.Green;               
                NewPassword = "";
                OldPassword = "";
                ConfirmNewPassword = "";
                _dataService.SaveItem<UserModel>(_user);
                _navigationService.Close(this);
                _result = true;

                
                
            }
            else if (string.IsNullOrEmpty(_newPassword)|| string.IsNullOrEmpty(_oldPassword) || string.IsNullOrEmpty(_confirmNewPassword))
            {
                AlertColor = UIColor.Red;
                AlertMessage = "Not all required fields was filled!";
                _result = false;
            }           
            else if (!IsOldPasswordValid)
            {
                AlertColor = UIColor.Red;
                AlertMessage = "Old password is wrong!";
                _result = false;
            }
            else if (!IsNewPasswordValid)
            {
                AlertColor = UIColor.Red;
                AlertMessage = "New password does not fit!";
                _result = false;
            }
            else if (_newPassword != _confirmNewPassword)
            {
                AlertColor = UIColor.Red;
                AlertMessage = "New and confirm password do not match!";
                _result = false;
            }
            else
            {
                AlertColor = UIColor.Red;
                AlertMessage = "Fields was filled incorrectly!";
                _result = false;
            }
            _model.Result += () => _result;
            _model.Begin();
        }
        #endregion
    }
}
