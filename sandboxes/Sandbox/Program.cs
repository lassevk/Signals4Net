using Signals4Net;

var context = new SignalContext();

IState<int> counter = context.State(0);
IComputed<bool> isEven = context.Computed(() => (counter.Value & 1) == 0);
IComputed<string> oddEven = context.Computed(() => isEven.Value ? "even" : "odd");

context.Effect(() => Console.WriteLine($"counter: {counter.Value}, oddEven state: {oddEven.Value}"));

// Simulate external updates to counter
new Timer(_ => counter.Value++, null, 1000, 1000);

Console.ReadLine();