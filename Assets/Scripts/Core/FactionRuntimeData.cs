using System.Collections.Generic;

public class FactionRuntimeData
{
	public FactionData baseData;

	// Œ»İ‘‹à
	public int gold;

	public FactionRuntimeData(
		FactionData data)
	{
		baseData = data;

		gold =
			data.startingGold;
	}
}