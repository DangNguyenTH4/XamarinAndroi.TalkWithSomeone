using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerTalk
{
    public class ChatHub : Hub
    {
        public static List<Room> Rooms { get; set; } = new List<Room>();

        public override async Task OnConnected()
        {
            await base.OnConnected();
            //Use Linq to search Room, that not enough Persion (Max 2)
            var room = Rooms.FirstOrDefault(r => r.Users.Count < 2);
            if (room != null)
            {
                //Each user connect to Hub, Have only one ConectionID
                room.Users.Add(Context.ConnectionId);
                if (room.Users.Count == 2)
                {
                    //Enough 2 persion to talk, send Ready Message to 2client
                    Clients.Clients(room.Users).Ready();
                }
            }
            else
            {
                room = new Room();
                room.Users.Add(Context.ConnectionId);
                Rooms.Add(room);
            }
        }
        public override Task OnDisconnected(bool stopCalled)
        {
            return base.OnDisconnected(stopCalled);
        }

        //When Recieve A Request Out Room
        public void OutOfRoom()
        {
            var room = Rooms.FirstOrDefault(x => x.Users.Contains(Context.ConnectionId));
            if (room != null)
            {
                //Send to all client message Out room
                Clients.Clients(room.Users).Message("OutRoom");
                Rooms.Remove(room);
            }
        }
        //When Recieve A Message
        public void Chat(string mes)
        {
            
            var room = Rooms.FirstOrDefault(x => x.Users.Contains(Context.ConnectionId));
            if (room != null)
            {
                //Send to all (2) client a message content : message content, and UserID of Sender.
                Clients.Clients(room.Users).Message(mes, Context.ConnectionId);
                if (mes.Equals(""))
                    Rooms.Remove(room);
            }
        }
    }
    public class Room
    {
        public string Id { get; set; }
        public List<string> Users { get; set; }
        public Room()
        {
            Id = Guid.NewGuid().ToString();
            Users = new List<string>();
        }
    }
}
