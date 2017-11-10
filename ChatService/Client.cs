using System.Collections.Generic;

namespace ChatService
{
    public class Client
    {
        public IClient Callback { get; set; }
        public string Id { get; set; }

        public List<Client> ConnectedUsers { get; set; } = new List<Client>();

        public Client(IClient callback, string id)
        {
            Callback = callback;
            Id = id;
        }


        protected bool Equals(Client other)
        {
            return string.Equals(Id, other.Id) && Equals(ConnectedUsers, other.ConnectedUsers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Client) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Id != null ? Id.GetHashCode() : 0) * 397) ^ (ConnectedUsers != null ? ConnectedUsers.GetHashCode() : 0);
            }
        }
    }
}