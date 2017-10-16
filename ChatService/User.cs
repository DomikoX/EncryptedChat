using System.Collections.Generic;

namespace ChatService
{
    public class User
    {
        public IClient Callback { get; set; }
        public string Id { get; set; }

        public List<User> ConnectedUsers { get; set; } = new List<User>();

        public User(IClient callback, string id)
        {
            Callback = callback;
            Id = id;
        }


        protected bool Equals(User other)
        {
            return string.Equals(Id, other.Id) && Equals(ConnectedUsers, other.ConnectedUsers);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((User) obj);
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