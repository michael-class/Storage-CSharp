
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Note: Shared assembly information is specified in SharedAssemblyInfo.cs

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Paycor.Storage.Azure")]
[assembly: AssemblyDescription("Azure blob storage service class library")]
[assembly: AssemblyCulture("")]

// expose internal methods to unit test fixture
[assembly: InternalsVisibleTo("Paycor.Storage.Azure.Tests")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("e5dcb7dc-4466-4191-b524-e94caf4256f1")]
