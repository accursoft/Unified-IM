using System;

using Bricksoft.IM.MSN;

namespace UnifiedIM
{
    class MsnContact : IContact, ISendTyping, ISendFile
    {
        readonly MSNBuddy contact;
        readonly MSNClient client;

        internal MsnContact(MSNBuddy contact, MSNClient client)
        {
            this.contact = contact;
            this.client = client;
        }

        public string Id { get { return contact.Email; } }

        public string DisplayName { get { return contact.Nickname; } }

        public ContactStatus Status
        {
            get
            {
                switch (contact.Status) {
                    case MSNStatus.msnAppearOffline: return ContactStatus.Offline;
                    case MSNStatus.msnBeRightBack:
                    case MSNStatus.msnBusy:
                    case MSNStatus.msnOnThePhone:
                    case MSNStatus.msnOutLunch:
                    case MSNStatus.msnAway: return ContactStatus.Unavailable;
                    case MSNStatus.msnIdle:
                    case MSNStatus.msnOnline: return ContactStatus.Online;
                    default: return ContactStatus.Custom;
                }
            }
        }

        public string StatusText
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(contact.PersonalMessage)) return contact.PersonalMessage;

                switch (contact.Status) {
                    case MSNStatus.msnAppearOffline: return "Offline";
                    case MSNStatus.msnAway: return "Away";
                    case MSNStatus.msnBeRightBack: return "Be Right Back";
                    case MSNStatus.msnBusy: return "Busy";
                    case MSNStatus.msnIdle: return "Idle";
                    case MSNStatus.msnOnline: return "Online";
                    case MSNStatus.msnOnThePhone: return "On The Phone";
                    case MSNStatus.msnOutLunch: return "Out To Lunch";
                    default: return string.Empty;
                }
            }
        }

        public void SendMessage(string message)
        {
            client.SendMessage(new TextMessage(message) { Buddy = Id });
        }

        internal MSNChatSession session;

        public void SendTyping()
        {
            if (session != null)
                session.SendTypingMessage();
        }

        public void SendFile(string path)
        {
            client.SendFile(Id, path);
        }

        public void Remove()
        {
            client.DeleteBuddy(Id);
        }
    }

    /// <summary>MSN connection.</summary>
    public class MsnConnection : Connection, IReceiveTyping, ICustomStatus
    {
        MSNClient msnClient { get { return (MSNClient) Client; } }

        /// <summary>Initializes a new instance of the <see cref="MsnConnection"/> class.</summary>
        /// <param name="account">The account.</param>
        /// <param name="password">The password.</param>
        public MsnConnection(string account, string password) : base(new MSNClient(), account, password)
        {
            msnClient.OnChatSessionBuddyTyping += (sender, session, buddy) => { IsTyping(this, new EventArgs<IContact>(_contacts[buddy])); };
            msnClient.OnChatSessionStart += (sender, session) => { ((MsnContact)_contacts[session.ChatPartner]).session = session; };
            msnClient.OnChatSessionStop += (sender, session) => { ((MsnContact) _contacts[session.ChatPartner]).session = null; };
        }

        protected override IContact ContactFactory(string buddy)
        {
            return new MsnContact((MSNBuddy) Client.BuddyList[buddy], msnClient);
        }

        /// <summary>Occurs when a contact is typing.</summary>
        public event EventHandler<EventArgs<IContact>> IsTyping = delegate { };

        /// <summary>Sets the connection's status.</summary>
        /// <exception cref="InvalidOperationException">Thrown when the status is not one of ContactStatus.Online, ContactStatus.Offline, or ContactStatus.Unavailable.</exception>
        /// <param name="status">The connection's status.</param>
        public override void SetStatus(ContactStatus status)
        {
            switch (status) {
                case ContactStatus.Online: msnClient.Status = MSNStatus.msnOnline;
                    break;
                case ContactStatus.Offline: msnClient.Status = MSNStatus.msnAppearOffline;
                    break;
                case ContactStatus.Unavailable: msnClient.Status = MSNStatus.msnBusy;
                    break;
                default: throw new InvalidOperationException();
            }
        }
        
        /// <summary>Sets a custom status.</summary>
        /// <param name="status">The connection's status.</param>
        public void SetCustomStatus(string status)
        {
            msnClient.PersonalMessage = status;
        }

        /// <summary>Adds a contact.</summary>
        /// <param name="id">The contact to add.</param>
        public override void AddContact(string id)
        {
            msnClient.AddBuddy(id);
        }
    }
}