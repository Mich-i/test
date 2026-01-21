namespace ChatTool.Shared
{
    public enum UserType
    {
        Me,
        Peer
    }
    public class ChatMessage
    {
        public string Message { get; }
        public string From { get; private set; }
        public UserType userType { get; private set; }

        public ChatMessage(string message, string from, UserType userType)
        {
            this.Message = message;
            this.From = from;
            this.userType = userType;
        }

        public void UpdateUserTypeAndSender()
        {
            this.userType = UserType.Peer;
            if (this.From == "Me") { this.From = "Peer"; }
        }
    }
}
