using System;
using System.Collections.Generic;

[Serializable]
public class ProvinceMasterData
{
	public string provinceId;
	public string provinceName;
	public string initialOwner;
	public bool isGate;
	//public List<ProvinceData> requiredProvinces =
	//	new List<ProvinceData>();

	public List<RequiredProvince>
		requiredProvinces;

	public int income;
	public int defenseValue;
	public int initialDefenseLevel;

	public List<string> neighbors =
		new List<string>();

	public List<string> initialCharacterIds;
		 //= new List<string>();

}

[Serializable]
public class ProvinceMasterDataList
{
	public List<ProvinceMasterData>
		provinces;
}

[System.Serializable]
public class RequiredProvince
{
	public string provinceId;
}