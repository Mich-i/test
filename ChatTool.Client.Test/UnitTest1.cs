using ChatTool.Core.Domain.Models;
using AwesomeAssertions;

namespace ChatTool.Core.Test
{
    public class UnitTest1
    {
        [Fact]
        public void CheckIfTheUserCanBeAddedCorrectly()
        {
            //Arrange
            UserInformation userInformation = new UserInformation();
            string name = "TestUser";
            string connectionId = "12345";

            //Act
            userInformation.AddUserToDictionary(name, connectionId);
            bool userExists = userInformation.CheckIfTheUserAlreadyExist(name, connectionId);

            //Assert
            userExists.Should().BeTrue();
        }
    }
}