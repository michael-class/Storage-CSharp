using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Note: Shared assembly information is specified in SharedAssemblyInfo.cs

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Paycor.Storage.Domain")]
[assembly: AssemblyDescription("Storage service domain class library")]
[assembly: AssemblyCulture("")]

// expose internal methods to unit test fixture
[assembly: InternalsVisibleTo("Paycor.Storage.Domain.Tests")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("515eef69-59a4-4b43-b62f-4592cc92ad58")]
