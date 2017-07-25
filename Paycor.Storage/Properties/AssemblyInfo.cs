using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Note: Shared assembly information is specified in SharedAssemblyInfo.cs

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Paycor.Storage")]
[assembly: AssemblyDescription("Storage service class library")]
[assembly: AssemblyCulture("")]

// expose internal methods to unit test fixture
[assembly: InternalsVisibleTo("Paycor.Storage.Tests")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("4d81b138-8224-413e-b74e-5b194a5e40d9")]
