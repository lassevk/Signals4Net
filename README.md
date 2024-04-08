Signals4Net
===

[![build](https://github.com/lassevk/Signals4Net/actions/workflows/build.yml/badge.svg)](https://github.com/lassevk/Signals4Net/actions/workflows/build.yml)
[![codecov](https://codecov.io/github/lassevk/Signals4Net/graph/badge.svg?token=M7F5JUBV7W)](https://codecov.io/github/lassevk/Signals4Net)
[![codeql](https://github.com/lassevk/Signals4Net/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/lassevk/Signals4Net/actions/workflows/github-code-scanning/codeql)

This project aim to release NuGet packages with a .NET implementation of [Signals](https://github.com/proposal-signals/proposal-signals), with similar features.

The project will not target specific workloads, so the goal is to be usable in almost all types of .NET projects.

**This project is currently under development, so anything and everything will change.**

Example
===

    using Signals4Net;
    
    var context = new SignalContext();
    
    IState<int> counter = context.State(0);
    IComputed<bool> isEven = context.Computed(async () => (await counter.GetValueAsync() & 1) == 0);
    IComputed<string> oddEven = context.Computed(async () => (await isEven.GetValueAsync()) ? "even" : "odd");
    
    await context.AddEffectAsync(async () => Console.WriteLine($"counter: {await counter.GetValueAsync()}, oddEven state: {await oddEven.GetValueAsync()}"));
    
    // Simulate external updates to counter
    new Timer(async _ => await counter.MutateAsync(v => v + 1), null, 1000, 1000);
    
    Console.ReadLine();

*(this is similar to the example in the JS proposal, found [here](https://github.com/proposal-signals/proposal-signals/tree/main?tab=readme-ov-file#example---a-signals-counter).)*
