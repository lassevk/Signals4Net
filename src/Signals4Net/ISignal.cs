using System.ComponentModel;

namespace Signals4Net;

/// <summary>
/// This interface is implemented by all signal objects and is used as the building
/// block for handling dependencies between computed variables and state variables.
/// </summary>
public interface ISignal : INotifyPropertyChanged
{
}