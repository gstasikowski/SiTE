using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiTE.Logic
{
    public class UserPasswordHandler
    {
        public UserPasswordHandler()
        {
            Views.PasswordInput passwordWindow = new Views.PasswordInput();
            passwordWindow.ShowDialog();
        }
    }
}
