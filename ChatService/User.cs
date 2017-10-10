using System.Collections.Generic;

namespace ChatService
{
    public class User
    {
        public IClient Callback { get; set; }
        public int Id { get; set; }

        public List<User> ConnectedUsers { get; set; } = new List<User>();

        public User(IClient callback, int id)
        {
            Callback = callback;
            Id = id;
        }


        protected bool Equals(User other)
        {
            return Equals(Callback, other.Callback) || Id == other.Id;
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
                return ((Callback != null ? Callback.GetHashCode() : 0) * 397) ^ Id;
            }
        }
    }
}