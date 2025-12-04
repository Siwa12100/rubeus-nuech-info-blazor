using NuitInfo.Rubeus.Modeles;
using Xunit;

namespace NuitInfo.Rubeus.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        ChatMessage msg = new()
        {
            Author = "TestAuthor",
            Text = "Hello, this is a test message!"
        };
        
        Assert.True(true);
    }
}
