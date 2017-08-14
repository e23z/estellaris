using System;
using System.IO;
using System.Text;
using System.Threading;

namespace Estellaris.Terminal {
  public class ProgressBar : IDisposable, IProgress<float> {
    const int blockCount = 10;
    const string animation = @"|/-\";

    static readonly object _lock = new object();
    readonly TimeSpan _animationInterval = TimeSpan.FromSeconds(1.0 / 8);
    readonly Timer _timer;
    readonly TextWriter _output;
    float _currProgress = 0;
    string _currText = "";
    bool _disposed = false;
    int _animationIndex = 0;

    public ProgressBar() {
      _timer = new Timer(TimerHandler, null, _animationInterval, TimeSpan.FromMilliseconds(-1));
      _output = Console.Out;

      if (!Console.IsOutputRedirected)
        ResetTimer();
    }

    public void Report(float value) {
      value = Math.Max(0, Math.Min(1, value));
      Interlocked.Exchange(ref _currProgress, value);
    }

    public void Report(float value, out string progressText) {
      Report(value);
      progressText = GetProgressText();
    }

    void TimerHandler(object state) {
      lock (_lock) {
        if (_disposed)
          return;
        UpdateText(GetProgressText());
        ResetTimer();
      }
    }

    string GetProgressText() {
      var progressBlockCount = (int) (_currProgress * blockCount);
      var percent = (int) (_currProgress * 100f);
      return string.Format("[{0}{1}] {2,3}% {3}",
        new string('#', progressBlockCount),
        new string('-', blockCount - progressBlockCount),
        percent,
        animation [_animationIndex++ % animation.Length]);
    }

    void UpdateText(string text) {
      var commonPrefixLength = 0;
      var commonLength = Math.Min(_currText.Length, text.Length);
      var overlapCount = _currText.Length - text.Length;

      while (commonPrefixLength < commonLength && text [commonPrefixLength] == _currText [commonPrefixLength])
        commonPrefixLength++;

      var outputBuilder = new StringBuilder();
      outputBuilder.Append('\b', _currText.Length - commonPrefixLength);
      outputBuilder.Append(text.Substring(commonPrefixLength));

      if (overlapCount > 0) {
        outputBuilder.Append(' ', overlapCount);
        outputBuilder.Append('\b', overlapCount);
      }

      _output.Write(outputBuilder);
      _currText = text;
    }

    void ResetTimer() {
      _timer.Change(_animationInterval, TimeSpan.FromMilliseconds(-1));
    }

    public void Dispose() {
      lock (_timer) {
        _disposed = true;
        UpdateText("");
      }
    }
  }
}
