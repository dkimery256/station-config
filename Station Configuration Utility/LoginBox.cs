/*
This class builds a login box.
*/

using System.Windows.Forms;
using System;
using System.Drawing;

namespace Station_Configuration_Utility
{
    class LoginBox
    {
        private string login;
        private string password;
        bool firstLogin = false;
        private string loginEntry;
        private string passwordEntry;
        private bool loginAttempt = false;
        private FileHandler reader = new FileHandler();
        private FileHandler writter = new FileHandler();
        string loginPath = "M:/Station Configs/System/error_log.txt";
        public bool loginInputBox(string title, string message, string loginType)
        {
            
            //read login info from file
            
            string results = reader.textReader(loginPath);

            if (results != null) {
                string[] resultArray = results.Split(',');
                login = resultArray[0];
                password = resultArray[1];
                if (login.Equals("admin") && password.Equals("password")) firstLogin = true;

                //if first login ask user to enter new password
                if (firstLogin)
                {
                    while (true)
                    {
                        DialogResult dialog = loginInputBox("Enter New Login and Password", "This is the first login, please enter new login and password.");
                        if (loginEntry != null && passwordEntry != null)
                        {
                            writter.clearText(loginPath);
                            writter.textWriter(loginPath, loginEntry + "," + passwordEntry);
                            loginAttempt = true;
                            MessageBox.Show(" Login and Password change was successful!", "Succes!");
                            break;
                        }
                        else if (dialog == DialogResult.Cancel)
                        {
                            loginAttempt = false;
                            break;
                        }
                        else
                        {
                            MessageBox.Show("Incorrect Login or Password");
                            continue;
                        }    
                    }
                }
                //Regualr login box
                else if (loginType.Equals("login"))
                {
                    while (true)
                    {
                        DialogResult dialog =  loginInputBox(title, message);
                        if ((loginEntry != null && loginEntry.Equals(login)) && (passwordEntry != null && passwordEntry.Equals(password)))
                        {
                            loginAttempt = true;
                            break;
                        }
                        else if (dialog == DialogResult.Cancel)
                        {
                            loginAttempt = false;
                            break;
                        }
                        else
                        {
                            MessageBox.Show("Incorrect Login or Password");
                            continue;
                        }
                    }
                }//New login box
                else if (loginType.Equals("newLogin"))
                {
                    while (true)
                    {
                        DialogResult dialog = loginInputBox(title, message);
                        if (loginEntry != null && passwordEntry != null )
                        {
                            writter.clearText(loginPath);
                            writter.textWriter(loginPath, loginEntry + "," + passwordEntry);
                            MessageBox.Show(" Login and Password change was successful!", "Succes!");
                            break;
                        }
                        else if (dialog == DialogResult.Cancel)
                        {
                            break;
                        }
                        
                    }
                }

            }
            //Data from login box was null or invalid
            else
            {
                loginAttempt = false;
            }
            return loginAttempt;

        }
        //Build actual text box for inputs
        public DialogResult loginInputBox(string title, string promptText)
        {
            Form form = new Form();
            Label titleLabel = new Label();
            Label loginLabel = new Label();
            Label passwordLabel = new Label();
            TextBox loginTextBox = new TextBox();
            TextBox passwordTextBox = new TextBox();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            titleLabel.Text = promptText;
            loginLabel.Text = "Login:";
            passwordLabel.Text = "Password:";
            passwordTextBox.PasswordChar = '*';
            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            titleLabel.SetBounds(10, 20, 375, 15);
            loginLabel.SetBounds(20, 55, 75, 20);
            loginTextBox.SetBounds(75, 50, 300, 20);
            passwordLabel.SetBounds(20, 80, 75, 20);
            passwordTextBox.SetBounds(75, 75, 300, 20);
            buttonOk.SetBounds(230, 115, 75, 25);
            buttonCancel.SetBounds(310, 115, 75, 25);

            titleLabel.AutoSize = true;
            loginLabel.AutoSize = true;
            passwordLabel.AutoSize = true;
            loginTextBox.Anchor = loginTextBox.Anchor | AnchorStyles.Right;
            passwordTextBox.Anchor = passwordTextBox.Anchor | AnchorStyles.Right;
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(400, 150);
            form.Controls.AddRange(new Control[] { titleLabel, loginLabel, passwordLabel, loginTextBox, passwordTextBox, buttonOk, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, titleLabel.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            loginEntry = loginTextBox.Text;
            passwordEntry = passwordTextBox.Text;
            return dialogResult;
        }
    }
}
