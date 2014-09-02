using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("protobuf-net-data-light")]
[assembly: AssemblyDescription("ProtoBuf Data Light: a lightweight IDataReader serializer. This library serializes an IDataReader to a binary format using Marc Gravell's protobuf-net.")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("Arjen Post")]
[assembly: AssemblyProduct("ProtoBuf Data Light: a lightweight IDataReader serializer")]
[assembly: AssemblyCopyright("Copyright © 2014")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: AssemblyInformationalVersion("1.0.0.0-beta")]
[assembly: ComVisible(false)]