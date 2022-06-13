namespace SiTE.Logic
{
    public class UserPasswordHandler
    {
        public bool canUnlockDatabase = false;

        public UserPasswordHandler()
        {
            Views.PasswordInput passwordWindow = new Views.PasswordInput();
            passwordWindow.ShowDialog();
            canUnlockDatabase = (bool)passwordWindow.DialogResult;
        }
    }
}
