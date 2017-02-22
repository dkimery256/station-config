/*
 *  This class builds a warning box, user can choose to disable warning box
 */

using System.Drawing;
using System.Windows.Forms;

namespace Station_Configuration_Utility
{
    class WarningBox
    {
        private CheckBox warnCheck = new CheckBox();
        private FileHandler fh = new FileHandler();
        private string warnPathStation = "C:/Station Configs/System/warning_standard_add.txt";
        private string warnPathAsset = "C:/Station Configs/System/warning_asset_add.txt";
        private string warnPathHead = "C:/Station Configs/System/warning_head_delete.txt";

        public void warningBox(string title, string promptText, string check, string warnType)
        {
            DialogResult result = InputBoxWarn(title, promptText, check);
            if (warnType.Equals("station"))
            {
                if (warnCheck.Checked == true)
                {
                    fh.clearText(warnPathStation);
                    fh.textWriter(warnPathStation, "checked");
                }
            }
            if (warnType.Equals("asset"))
            {
                if (warnCheck.Checked == true)
                {
                    fh.clearText(warnPathAsset);
                    fh.textWriter(warnPathAsset, "checked");
                }
            }
            if (warnType.Equals("head"))
            {
                if (warnCheck.Checked == true)
                {
                    fh.clearText(warnPathHead);
                    fh.textWriter(warnPathHead, "checked");
                }
            }

        }
        
        
        //Build actual text box for inputs
        private DialogResult InputBoxWarn(string title, string promptText, string check)
        {
            Form form = new Form();
            Label prompt = new Label();
            Button buttonOk = new Button();
            Button buttonCancel = new Button();
            
            warnCheck.AutoSize = true;
            warnCheck.Text = check;
            warnCheck.UseVisualStyleBackColor = true;

            form.Text = title;
            prompt.Text = promptText;
            buttonOk.Text = "OK";
            buttonCancel.Text = "Cancel";
            buttonOk.DialogResult = DialogResult.OK;
            buttonCancel.DialogResult = DialogResult.Cancel;

            
            buttonOk.SetBounds(230, 115, 75, 25);
            buttonCancel.SetBounds(310, 115, 75, 25);
            prompt.SetBounds(50, 25, 325, 15);
            warnCheck.SetBounds(50, 65, 150, 15);
            
            buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            

            form.ClientSize = new Size(400, 150);
            form.Controls.AddRange(new Control[] { buttonOk, buttonCancel, prompt, warnCheck });
            form.FormBorderStyle = FormBorderStyle.FixedDialog;
            form.StartPosition = FormStartPosition.CenterScreen;
            form.MinimizeBox = false;
            form.MaximizeBox = false;
            form.AcceptButton = buttonOk;
            form.CancelButton = buttonCancel;

            DialogResult dialogResult = form.ShowDialog();
            return dialogResult;
        }
    }
}
