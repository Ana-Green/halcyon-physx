# Halcyon PhysX
Halcyon-tuned C# conversion of [NVIDIA's PhysX SDK](https://github.com/NVIDIAGameWorks/PhysX).

This is a reporting PhysX from C++ to C# using newer tools.

## Building
At the moment this has only been developped for Linux, though Windows is on the radar. MacOS is right off the edge of the horizon, but I think I can see its mast.

1. Bootstrap the project:
    ```bash
    $ ./bootstrap.sh
    ```
1. Open the folder in Visual Studio Code, or the solution file in your editor of choice.
1. Compile and run it.
1. Review the run log and generated files in the `bin` folder for problems.
1. Not gotten past that last point yet...

## Related tools

There are other ports of NVIDIA's PhysX SDK to C# out there, so why this one? It comes down to the other ports either being abandoned with an ancient version of PhysX or being forced to use legacy ways of doing things that are slowing them down.

* [Halcyon's own PhysX.NET](https://github.com/HalcyonGrid/PhysX.net) - Quite ancient as it was forked from some early edition of stilldesign's PhysX.Net.  Doesn't help that the fork version and subsequent commits are lost to history.
* [StillDesign's PhysX.Net](https://github.com/stilldesign/PhysX.Net) - A nice clean and fairly up-to-date port.  That said, it uses the [nearly obsolete C++/CLI](https://github.com/dotnet/coreclr/issues/659) interface to work with the PhysX library and [they don't have the time to rebuild the port so that they can gain .net Core support](https://github.com/stilldesign/PhysX.Net/issues/38).  I know that pain.
* [PhysX Candy Wrapper](http://eyecm-physx.sourceforge.net/) - Looks very dead, nothing new in about a decade.

I'm sure there have been others I've tripped across, but StillDesign's work has far outlasted and outshined them all. This is simply our own attempt to take [stilldesign's comment](https://github.com/stilldesign/PhysX.Net/issues/38#issuecomment-451846820) to heart and give it a go.