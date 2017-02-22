using System;
using System.Drawing;
using System.Windows.Forms;
using Outlook = Microsoft.Office.Interop.Outlook;

namespace Station_Configuration_Utility
{
    class Email
    {

        private FileHandler fh = new FileHandler();
        private string emailPath = "M:/Station Configs/System/email.txt";
        private string message;

        public void createEmail(string title, string text)
        {
            DialogResult dialog = emailInputBox(title, text);
            if(dialog == DialogResult.OK)
            {
                sendEmail();
            }
            else if(dialog == DialogResult.Cancel)
            {
                MessageBox.Show("Email was canceled.", "Canceled");
            }
        }

        public void sendEmail()
        {

            try
            {
                string emailAddress = fh.textReader(emailPath);
                Outlook._Application _app = new Outlook.Application();
                Outlook.MailItem mail = (Outlook.MailItem)_app.CreateItem(Outlook.OlItemType.olMailItem);
                mail.To = emailAddress;
                mail.Subject = "Station Config Util Help Request";
                mail.Body = message;
                mail.Importance = Outlook.OlImportance.olImportanceNormal;
                ((Outlook._MailItem)mail).Send();
                MessageBox.Show("Email was successfully sent!", "Success!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Message", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            
        }

        //Build actual text box for inputs
        public DialogResult emailInputBox(string title, string promptText)
        {
            Form form = new Form();
            Label enterMessageLabel = new Label();            
            TextBox messageTextBox = new TextBox();
            Button buttonSend = new Button();
            Button buttonCancel = new Button();

            form.Text = title;
            enterMessageLabel.Text = promptText;
            buttonSend.Text = "Send";
            buttonCancel.Text = "Cancel";
            buttonSend.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;
            
            messageTextBox.Multiline = true;            
            messageTextBox.ScrollBars = ScrollBars.Vertical;
            messageTextBox.AcceptsReturn = true;
            messageTextBox.AcceptsTab = true;            
            messageTextBox.WordWrap = true;

            enterMessageLabel.SetBounds(10, 20, 375, 15);
            messageTextBox.SetBounds(10, 40, 350, 300);
            buttonSend.SetBounds(230, 350, 75, 25);
            buttonCancel.SetBounds(310, 350, 75, 25);

            enterMessageLabel.AutoSize = true;
            messageTextBox.Anchor = messageTextBox.Anchor | AnchorStyles.Right;
            buttonSend.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

            form.ClientSize = new Size(400, 400);
            form.Controls.AddRange(new Control[] { messageTextBox, enterMessageLabel, buttonSend, buttonCancel });
            form.ClientSize = new Size(Math.Max(300, enterMessageLabel.Right + 10), form.ClientSize.Height);
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonSend;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            message = messageTextBox.Text;
            return dialogResult;
        }
    }
}
