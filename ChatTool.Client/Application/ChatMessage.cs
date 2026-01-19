namespace ChatTool.Client.Application
{
    public enum UserType
    {
        Me,
        Peer
    }
    public class ChatMessage
    {
        public string Message { get; set; }
        public string From { get; set; }
        public UserType userType { get; set; }

        public ChatMessage(string message, string from, UserType userType)
        {
            this.Message = message;
            this.From = from;
            this.userType = userType;
        }
    }
}
