using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ChatClient
{
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private string username;

        public MainWindow()
        {
            InitializeComponent();
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            client = new TcpClient("127.0.0.1", 5000);
            stream = client.GetStream();
            username = PromptUsername();
            byte[] buffer = Encoding.UTF8.GetBytes(username);
            stream.Write(buffer, 0, buffer.Length);

            Thread receiveThread = new Thread(ReceiveMessages);
            receiveThread.Start();
        }

        private string PromptUsername()
        {
          
            InputDialog inputDialog = new InputDialog("Введите имя пользователя:");
            if (inputDialog.ShowDialog() == true)
            {
                return inputDialog.Answer;
            }
            return "Unknown";
        }

        private void ReceiveMessages()
        {
            while (true)
            {
                try
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Dispatcher.Invoke(() =>
                    {
                        ChatTextBox.AppendText($"{message}\n");
                    });
                }
                catch
                {
                    break;
                }
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string recipient = RecipientTextBox.Text;
                string message = MessageTextBox.Text;
                if (string.IsNullOrWhiteSpace(recipient) || string.IsNullOrWhiteSpace(message))
                {
                    MessageBox.Show("Заполните все поля.");
                    return;
                }

                string fullMessage = $"{recipient}:{message}";
                byte[] buffer = Encoding.UTF8.GetBytes(fullMessage);
                stream.Write(buffer, 0, buffer.Length);
                MessageTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при отправке сообщения: {ex.Message}");
            }
        }
    }

    public class InputDialog : Window
    {
        private TextBox inputTextBox;

        public InputDialog(string question)
        {
            Title = question;
            Width = 300;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            inputTextBox = new TextBox { Margin = new Thickness(10) };
            Button okButton = new Button { Content = "OK", Width = 60, Height = 25, Margin = new Thickness(10) };
            okButton.Click += OkButton_Click;
            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(inputTextBox);
            stackPanel.Children.Add(okButton);
            Content = stackPanel;
        }

        public string Answer => inputTextBox.Text;

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
