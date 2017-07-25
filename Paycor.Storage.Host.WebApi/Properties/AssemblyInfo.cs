using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Note: Shared assembly information is specified in SharedAssemblyInfo.cs

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Paycor.Storage.Host.WebApi")]
[assembly: AssemblyDescription("Storage service Web API")]
[assembly: AssemblyCulture("")]

// expose internal methods to unit test fixture
[assembly: InternalsVisibleTo("Paycor.Storage.Host.WebApi.Tests")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("eb565f01-544f-4580-9eb1-2c02852be4ef")]
