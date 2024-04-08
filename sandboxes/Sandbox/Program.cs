using Signals4Net;

var context = new SignalContext();

IState<int> counter = context.State(0);
IComputed<bool> isEven = context.Computed(async () => (await counter.GetValueAsync() & 1) == 0);
IComputed<string> oddEven = context.Computed(async () => (await isEven.GetValueAsync()) ? "even" : "odd");

_ = context.AddEffectAsync(async () => Console.WriteLine($"counter: {await counter.GetValueAsync()}, oddEven state: {await oddEven.GetValueAsync()}"));

// Simulate external updates to counter
new Timer(async _ => await counter.SetValueAsync(await counter.GetValueAsync() + 1), null, 1000, 1000);

Console.ReadLine();