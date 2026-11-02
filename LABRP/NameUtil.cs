using System.Text;

namespace LabRP;

public static class NameUtil
{
	public const int MaxLength = 32;

	public static string Clean(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return "";
		}
		StringBuilder stringBuilder = new StringBuilder(name.Length);
		foreach (char c in name)
		{
			if (c != '<' && c != '>' && !char.IsControl(c))
			{
				stringBuilder.Append(c);
				if (stringBuilder.Length >= 32)
				{
					break;
				}
			}
		}
		return stringBuilder.ToString().Trim();
	}
}
