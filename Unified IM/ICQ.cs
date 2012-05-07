using System;

using Bricksoft.IM.ICQ;

namespace UnifiedIM
{
    class IcqContact : IContact
    {
        readonly ICQBuddy contact;
        readonly ICQClient client;

        internal IcqContact(ICQBuddy contact, ICQClient client)
        {
            this.contact = contact;
            this.client = client;
        }

        public string Id { get { return contact.Account; } }

        public string DisplayName { get { return contact.Nickname; } }

        public ContactStatus Status
        {
            get
            {
                switch (contact.Status) {
                    case ICQStatus.icqAway:
                    case ICQStatus.icqBusy: return ContactStatus.Unavailable;
                    case ICQStatus.icqOffline: return ContactStatus.Offline;
                    case ICQStatus.icqOnline: return ContactStatus.Online;
                    default: return ContactStatus.Custom;
                }
            }
        }

        public string StatusText
        {
            get
            {
                switch (contact.Status) {
                    case ICQStatus.icqAway: return "Away";
                    case ICQStatus.icqBusy: return "Busy";
                    case ICQStatus.icqOffline: return "Offline";
                    case ICQStatus.icqOnline: return "Online";
                    default: return string.Empty;
                }
            }
        }

        public void SendMessage(string message)
        {
            client.SendMessage(Id, message);
        }

        public void Remove()
        {
            client.DeleteBuddy(Id);
        }
    }

    /// <summary>ICQ connection.</summary>
    public class IcqConnection : Connection
    {
        ICQClient icqClient { get { return (ICQClient) Client; } }

        /// <summary>Initializes a new instance of the <see cref="IcqConnection"/> class.</summary>
        /// <param name="account"> The account.</param>
        /// <param name="password">The password.</param>
        public IcqConnection(string account, string password) : base(new ICQClient(), account, password) { }

        protected override IContact ContactFactory(string buddy)
        {
            return new IcqContact((ICQBuddy) Client.BuddyList[buddy], icqClient);
        }

        /// <summary>Sets the connection's status.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the status is not one of ContactStatus.Online, ContactStatus.Offline, or ContactStatus.Unavailable.</exception>
        /// <param name="status">The connection's status.</param>
        public override void SetStatus(ContactStatus status)
        {
            switch (status) {
                case ContactStatus.Online: icqClient.Status = ICQStatus.icqOnline;
                    break;
                case ContactStatus.Offline: icqClient.Status = ICQStatus.icqOffline;
                    break;
                case ContactStatus.Unavailable: icqClient.Status = ICQStatus.icqBusy;
                    break;
                default: throw new InvalidOperationException();
            }
        }

        /// <summary>Adds a contact.</summary>
        /// <param name="id">The contact to add.</param>
        public override void AddContact(string id)
        {
            icqClient.AddBuddy(id);
        }
    }
}