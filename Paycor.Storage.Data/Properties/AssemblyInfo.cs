using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Note: Shared assembly information is specified in SharedAssemblyInfo.cs

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Paycor.Storage.Data")]
[assembly: AssemblyDescription("Storage service data/repository class library")]
[assembly: AssemblyCulture("")]

// expose internal methods to unit test fixture
[assembly: InternalsVisibleTo("Paycor.Storage.Data.Tests")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("8fbd3869-222b-4769-b131-00f261b3ecc5")]
