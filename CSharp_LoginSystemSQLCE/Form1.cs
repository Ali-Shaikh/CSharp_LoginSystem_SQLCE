using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;


namespace CSharp_LoginSystemSQLCE
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void usersBindingNavigatorSaveItem_Click(object sender, EventArgs e)
        {
            this.Validate();
            this.usersBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.loginsDataSet);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'loginsDataSet.Users' table. You can move, or remove it, as needed.
            this.usersTableAdapter.Fill(this.loginsDataSet.Users);

        }

        private void AddUser(string username, string password, string confirmPass, string email)
        {
            //Local variables to hold values
            string smtpEmail = txtSMTPUsername.Text;
            string smtpPassword = txtSMTPPassword.Text;
            int smtpPort = (int)numUDSMTPPort.Value;
            string smtpAddress = txtSMTPAddress.Text;

            //Regex for making sure Email is valid
            Regex regex = new Regex(@"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$");
            Match match = regex.Match(email);

            //Loop through Logins Table
            foreach (DataRow row in loginsDataSet.Users)
            {
                //And look for matching usernames
                if (row.ItemArray[0].Equals(username))
                {
                    //If one is found, show message:
                    MessageBox.Show("Username already exists");
                    return;
                }
            }

            //Confirm pass must equal password.
            if (password != confirmPass)
            {
                MessageBox.Show("Passwords do not match");
            }
            //Password must be at least 8 characters long
            else if (password.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters long");
            }
            //If email is NOT valid
            else if (!match.Success)
            {
                MessageBox.Show("Invalid Email");
            }
            //If there is no username
            else if (username == null)
            {
                MessageBox.Show("Must have Username");
            }
            //If all is well, create the new user!
            else
            {
                loginsDataSet.UsersRow newUserRow = loginsDataSet.Users.NewUsersRow();

                string EncryptedPass = HashPass(password, email);
                newUserRow.Username = username;
                newUserRow.Password = EncryptedPass;
                newUserRow.Email = email;

                loginsDataSet.Users.Rows.Add(newUserRow);
                txtRegisterUsername.Text = String.Empty;
               txtRegisterPassword.Text = String.Empty;
                txtRegisterConfirmPassword.Text = String.Empty;
                txtRegisterEmail.Text = String.Empty;
                MessageBox.Show("Thank you for Registering!");
                if (String.IsNullOrWhiteSpace(smtpEmail) || String.IsNullOrWhiteSpace(smtpPassword) || String.IsNullOrWhiteSpace(smtpAddress) || smtpPort <= 0)
                {
                    MessageBox.Show("Email configuration is not set up correctly! \nCannot sent email!");

                }
                else
                {
                    SendMessage(email.ToString(), username.ToString(), password.ToString());
                }
            }
        }

        public string HashPass(string password, string email)
        {
            SHA256 sha = new SHA256CryptoServiceProvider();

            //compute hash from the bytes of text
            sha.ComputeHash(ASCIIEncoding.ASCII.GetBytes(password + email));

            //get hash result after compute it
            byte[] result = sha.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits
                //for each byte
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }

        public void SendMessage(string ToAddress, string ToName, string password)
        {

            var client = new SmtpClient(txtSMTPAddress.Text, (int)numUDSMTPPort.Value)
            {
                Credentials = new NetworkCredential(txtSMTPUsername.Text, txtSMTPPassword.Text),
                EnableSsl = true
            };
            client.Send(txtSMTPUsername.Text, ToAddress, "Thank You!", "Thank you for registering with us today! \n Your username/passwords are: \n \nUsername: "
                                                                        + ToName.ToString() + "\nPassword: " + password.ToString());
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            AddUser(txtRegisterUsername.Text, txtRegisterPassword.Text, txtRegisterConfirmPassword.Text, txtRegisterEmail.Text);
        }
    }
}
