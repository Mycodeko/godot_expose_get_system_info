using Godot;

public partial class CSharpRunner : RichTextLabel {
	public override void _Ready() {
		this.Text = "C#\n" + SystemInfo.GetSystemInfo();
	}
}
