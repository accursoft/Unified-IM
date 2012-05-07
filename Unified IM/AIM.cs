using System;

using Bricksoft.IM.AIM;

namespace UnifiedIM
{
    class AimContact : IContact
    {
        readonly AIMBuddy contact;
        readonly AIMClient client;

        internal AimContact(AIMBuddy contact, AIMClient client)
        {
            this.contact = contact;
            this.client = client;
        }

        public string Id { get { return contact.Account; } }

        public string DisplayName { get { return contact.Account; } }

        public ContactStatus Status
        {
            get
            {
                switch (contact.Status) {
                    case AIMStatus.aimAway: return ContactStatus.Unavailable;
                    case AIMStatus.aimInvisible:
                    case AIMStatus.aimOffline: return ContactStatus.Offline;
                    case AIMStatus.aimOnline: return ContactStatus.Online;
                    default: return ContactStatus.Custom;
                }

            }
        }

        public string StatusText
        {
            get
            {
                switch (contact.Status) {
                    case AIMStatus.aimAway: return "Away";
                    case AIMStatus.aimInvisible: return "Invisible";
                    case AIMStatus.aimOffline: return "Offline";
                    case AIMStatus.aimOnline: return "Online";
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
            throw new NotImplementedException();
        }
    }

    /// <summary>AIM connection.</summary>
    public class AimConnection : Connection
    {
        AIMClient aimClient { get { return (AIMClient) Client; } }

        /// <summary>Initializes a new instance of the <see cref="AimConnection"/> class.</summary>
        /// <param name="account">The account.</param>
        /// <param name="password">The password.</param>
        public AimConnection(string account, string password) : base(new AIMClient(), account, password) { }

        protected override IContact ContactFactory(string buddy)
        {
            return new AimContact((AIMBuddy) Client.BuddyList[buddy], aimClient);
        }

        /// <summary>Sets the connection's status.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the status is not one of ContactStatus.Online, ContactStatus.Offline, or ContactStatus.Unavailable.</exception>
        /// <param name="status">The connection's status.</param>
        public override void SetStatus(ContactStatus status)
        {
            switch (status) {
                case ContactStatus.Online: aimClient.Status = AIMStatus.aimOnline;
                    break;
                case ContactStatus.Offline: aimClient.Status = AIMStatus.aimOffline;
                    break;
                case ContactStatus.Unavailable: aimClient.Status = AIMStatus.aimAway;
                    break;
                default: throw new InvalidOperationException();
            }
        }

        /// <summary>Adds a contact.</summary>
        /// <exception cref="NotImplementedException">Contact management is not currently implemented for AIM.</exception>
        /// <param name="id">The contact to add.</param>
        public override void AddContact(string id)
        {
            throw new NotImplementedException();
        }
    }
}