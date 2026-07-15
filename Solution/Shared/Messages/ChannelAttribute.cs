using System;

namespace Shared.Messages
{
    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class ChannelAttribute : System.Attribute
    {
        public string TopicName;

        public ChannelAttribute(string name)
        {
            TopicName = name;
        }
    }
}
