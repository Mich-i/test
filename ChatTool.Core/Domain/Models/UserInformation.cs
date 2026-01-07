namespace ChatTool.Core.Domain.Models
{
    public class UserInformation
    {
        public Dictionary<string, string> Users { get; private set; } = new();

        public bool CheckIfTheUserAlreadyExist(string userName, string userConnectionId)
        {
            if (this.Users.ContainsKey(userName) || this.Users.ContainsValue(userConnectionId))
            {
                return true;
            }

            return false;
        }

        public void AddUserToDictionary(string userName, string userConnectionId)
        {
            this.Users.Add(userName, userConnectionId);
        }

        public void ChangeConnectionIdFromExistingUser(string userName, string newUserConnectionId)
        {
            this.Users[userName] = newUserConnectionId;
        }
    }
}