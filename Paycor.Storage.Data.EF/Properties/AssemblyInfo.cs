using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Note: Shared assembly information is specified in SharedAssemblyInfo.cs

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Paycor.Storage.Data.EF")]
[assembly: AssemblyDescription("Storage service Enitity Framework data class library")]
[assembly: AssemblyCulture("")]

// expose internal methods to unit test fixture
[assembly: InternalsVisibleTo("Paycor.Storage.Data.EF.Tests")]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("fdefad73-4d8c-411e-869f-b054a18b5c63")]
