using System;
using System.Runtime.Serialization;

namespace Artemis.Plugins.BuildTask
{
    [DataContract]
    [Serializable]
    public class PluginInfo
    {
        [DataMember]
        public string Guid;

        [DataMember]
        public string Name;

        [DataMember]
        public string Description;

        [DataMember]
        public string Author;

        [DataMember]
        public string Website;

        [DataMember]
        public string Repository;

        [DataMember]
        public string Version;

        [DataMember]
        public string Main;
    }
}
