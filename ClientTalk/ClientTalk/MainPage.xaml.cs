using Microsoft.AspNet.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ClientTalk
{
    public class Message
    {
        public string Content { get; set; }
        public bool IsMine { get; set; }
    }
    public partial class MainPage : ContentPage
    {
        IHubProxy ChatHub { get; }
        HubConnection HubConnect{ get; }

        public ObservableCollection<Message> CollectionMessage { get; set; }
        ////5.1
        //string URLServer = "http://192.168.56.1:5000";
        //7.0
        string URLServer = "http://192.168.114.2:5000";
        public MainPage()
        {
            InitializeComponent();
            CollectionMessage = new ObservableCollection<Message>();
            HubConnect = new HubConnection(URLServer);
            ChatHub = HubConnect.CreateHubProxy("ChatHub");
            ChatHub.On("Ready", OnReady);
            ChatHub.On<string, string>("Message", OnMessage);
            MainListView.ItemsSource = CollectionMessage;
        }

        public bool IsReady { get; set; } = false;
        private  void OnMessage(string mes, string userID)
        {
            if (mes.Equals(""))
            {
                OutRoomVoid();
                //If don't have this line, user Second will not be Stop HubConnect,
                //But if have this line, Will be disconnected, before done line 104 (await ChatHub.Invoke(chat..)
                if(!IGetOut)
                {
                    if (HubConnect.State == ConnectionState.Connected)
                        HubConnect.Stop();
                }

            }
            else
            {
                Device.BeginInvokeOnMainThread(() => {
                    var msg = new Message() { Content = mes, IsMine = userID == HubConnect.ConnectionId };
                    CollectionMessage.Add(msg);
                });

            }
        }
        private void OnReady()
        {
            IsReady = true;
            UpdateStatus("Đã đủ người rồi nói chuyện thôi");

        }

        #region Element/View
        #endregion
        private async void Button_Clicked(object sender, EventArgs e)
        {
            
            await HubConnect.Start();
            GetInARoomVoid();
            if (!IsReady)
                UpdateStatus("Đã vào phòng");
        }
        private void UpdateStatus(string message)
        {
            Device.BeginInvokeOnMainThread(() => {
                Statuss.Text = message;
            });
        }
        private void OnTwoPersionInRoom()
        {
            UpdateStatus("Đã đủ người rồi nói chuyện thôi");
            Thread.Sleep(3000);
            UpdateStatus("Rời phòng");
        }
        private async void btn_onSendClicked(object sender, EventArgs e)
        {
            if (ChatEntry.Text.Equals("") || !IsReady)
            {
                return;
            }
            SendButton.IsEnabled = false;
            await ChatHub.Invoke("Chat", ChatEntry.Text);
            ChatEntry.Text = string.Empty;
            SendButton.IsEnabled = true;
        }

        bool IGetOut = false;
        private  async void btn_onGetOutClicked(object sender, EventArgs e)
        {
            IGetOut = true;
            OutRoom.IsEnabled = false;
            try
            {
                await ChatHub.Invoke("Chat", "");
            }
            catch { }
            //Wait for get Message "GetOut", then Stop Connect the HubConnect.
            //If, HubConnect.Stop() in line 130, Will be crash, because the asyncho, (stop Hub, still...)
            if (HubConnect.State==ConnectionState.Connected)
                HubConnect.Stop();
        }
        private void GetInARoomVoid()
        {
            IGetOut = false;
            Device.BeginInvokeOnMainThread(() =>
            {
                MainButton.IsEnabled = false;
                OutRoom.IsEnabled = true;
                SendButton.IsEnabled = true;
            });
        }
        private void OutRoomVoid()
        {
            Device.BeginInvokeOnMainThread(() => {
                MainButton.IsEnabled = true;
                OutRoom.IsEnabled = false;
                SendButton.IsEnabled = false;
                IsReady = false;
                CollectionMessage.Clear();
                UpdateStatus("......");
            });
            //HubConnect.Stop();
        }
    }
}
