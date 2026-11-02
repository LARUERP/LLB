using System;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSLZ.Marrow;
using Il2CppTMPro;
using LabFusion.Data;
using LabFusion.Network;
using LabRP.Economy;
using LabRP.Network;
using MelonLoader;
using UnityEngine;
using UnityEngine.UI;

namespace LabRP.UI;

public static class WristHud
{
	private static GameObject _root;

	private static Transform _rootTransform;

	private static TextMeshPro _balanceText;

	private static TextMeshPro _popupText;

	private static TMP_FontAsset _font;

	private static long _targetBalance = 0L;

	private static double _displayedBalance = 0.0;

	private static long _lastDrawnBalance = -1L;

	private static float _popupTimer = 0f;

	private const float PopupDuration = 3.5f;

	private static readonly Color MoneyGreen = new Color(0.45f, 1f, 0.6f, 1f);

	private static readonly Color SoftRed = new Color(1f, 0.42f, 0.42f, 1f);

	private static readonly Color HostGold = new Color(1f, 0.85f, 0.4f, 1f);

	public static void OnLocalRigCreated()
	{
		_root = null;
		_rootTransform = null;
		_balanceText = null;
		_popupText = null;
		TryBuild();
	}

	public static void OnJoinedLobby()
	{
		_targetBalance = ClientEconomy.LocalBalance;
		_displayedBalance = _targetBalance;
		_popupTimer = 0f;
		TryBuild();
	}

	public static void OnLeftLobby()
	{
		_targetBalance = 0L;
		_displayedBalance = 0.0;
		DestroyHud();
	}

	public static void OnUpdate()
	{
		if (!NetworkInfo.HasServer)
		{
			if ((Object)(object)_root != (Object)null)
			{
				DestroyHud();
			}
			return;
		}
		if (!LabRPConfig.HudEnabled.Value)
		{
			if ((Object)(object)_root != (Object)null && _root.activeSelf)
			{
				_root.SetActive(false);
			}
			return;
		}
		if ((Object)(object)_root != (Object)null && ((Object)(object)_balanceText == (Object)null || (Object)(object)_popupText == (Object)null))
		{
			DestroyHud();
		}
		if ((Object)(object)_root == (Object)null)
		{
			TryBuild();
			if ((Object)(object)_root == (Object)null)
			{
				return;
			}
		}
		if (!_root.activeSelf)
		{
			_root.SetActive(true);
		}
		AnimateBalance();
		AnimatePopup();
		Billboard();
	}

	public static void OnLocalBalanceChanged(long newBalance, long delta, BalanceReason reason, string detail)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		_targetBalance = newBalance;
		detail = NameUtil.Clean(detail);
		switch (reason)
		{
		case BalanceReason.Initial:
			_displayedBalance = newBalance;
			return;
		case BalanceReason.Denied:
			ShowPopup(detail, SoftRed);
			return;
		}
		if (delta != 0L)
		{
			switch (reason)
			{
			case BalanceReason.Paycheck:
				ShowPopup("+$" + FormatAbs(delta) + "  Paycheck", MoneyGreen);
				break;
			case BalanceReason.PaymentReceived:
				ShowPopup("+$" + FormatAbs(delta) + "  from " + detail, MoneyGreen);
				break;
			case BalanceReason.PaymentSent:
				ShowPopup("-$" + FormatAbs(delta) + "  to " + detail, SoftRed);
				break;
			case BalanceReason.Admin:
				ShowPopup((delta >= 0) ? ("+$" + FormatAbs(delta) + "  Host") : ("-$" + FormatAbs(delta) + "  Host"), HostGold);
				break;
			}
		}
	}

	private static string FormatAbs(long value)
	{
		if (value == long.MinValue)
		{
			value = long.MaxValue;
		}
		return Math.Abs(value).ToString("N0");
	}

	private static void TryBuild()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Expected O, but got Unknown
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_root != (Object)null) && NetworkInfo.HasServer && LabRPConfig.HudEnabled.Value && RigData.HasPlayer)
		{
			Hand leftHand = RigData.Refs.LeftHand;
			if (!((Object)(object)leftHand == (Object)null))
			{
				TMP_FontAsset font = GetFont();
				_root = new GameObject("LabRP_WristHUD");
				_rootTransform = _root.transform;
				_rootTransform.SetParent(((Component)leftHand).transform, false);
				ApplyTransformPrefs();
				_balanceText = CreateText("BalanceText", font, 30f, MoneyGreen, (FontStyles)1);
				_balanceText.transform.localPosition = Vector3.zero;
				_popupText = CreateText("PopupText", font, 15f, MoneyGreen, (FontStyles)0);
				_popupText.transform.localPosition = new Vector3(0f, -3f, 0f);
				SetAlpha((TMP_Text)(object)_popupText, 0f);
				_displayedBalance = _targetBalance;
				_lastDrawnBalance = -1L;
				DrawBalance((long)_displayedBalance);
			}
		}
	}

	private static TextMeshPro CreateText(string name, TMP_FontAsset font, float fontSize, Color color, FontStyles style)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject(name);
		val.transform.SetParent(_root.transform, false);
		val.transform.localPosition = Vector3.zero;
		val.transform.localRotation = Quaternion.identity;
		val.transform.localScale = Vector3.one;
		TextMeshPro val2 = val.AddComponent<TextMeshPro>();
		if ((Object)(object)font != (Object)null)
		{
			((TMP_Text)val2).font = font;
		}
		((TMP_Text)val2).fontSize = fontSize;
		((TMP_Text)val2).fontStyle = style;
		((Graphic)val2).color = color;
		((TMP_Text)val2).rectTransform.sizeDelta = new Vector2(40f, 10f);
		((TMP_Text)val2).enableWordWrapping = false;
		((TMP_Text)val2).text = "";
		((TMP_Text)val2).alignment = (TextAlignmentOptions)514;
		((TMP_Text)val2).alignment = (TextAlignmentOptions)514;
		return val2;
	}

	public static void ApplyTransformPrefs()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_rootTransform == (Object)null))
		{
			_rootTransform.localPosition = new Vector3(LabRPConfig.HudOffsetX.Value, LabRPConfig.HudOffsetY.Value, LabRPConfig.HudOffsetZ.Value);
			_rootTransform.localRotation = Quaternion.Euler(0f, LabRPConfig.HudYaw.Value, 0f);
			float num = 0.01f * Mathf.Clamp(LabRPConfig.HudScale.Value, 0.1f, 6f);
			_rootTransform.localScale = new Vector3(num, num, num);
		}
	}

	private static void DestroyHud()
	{
		if ((Object)(object)_root != (Object)null)
		{
			Object.Destroy((Object)(object)_root);
		}
		_root = null;
		_rootTransform = null;
		_balanceText = null;
		_popupText = null;
		_popupTimer = 0f;
	}

	private static void AnimateBalance()
	{
		if ((Object)(object)_balanceText == (Object)null)
		{
			return;
		}
		double value = (double)_targetBalance - _displayedBalance;
		if (Math.Abs(value) < 0.5)
		{
			_displayedBalance = _targetBalance;
		}
		else
		{
			double num = Math.Max(Math.Abs(value) * 6.0, 40.0) * (double)Time.unscaledDeltaTime;
			if (num >= Math.Abs(value))
			{
				_displayedBalance = _targetBalance;
			}
			else
			{
				_displayedBalance += (double)Math.Sign(value) * num;
			}
		}
		DrawBalance((long)Math.Round(_displayedBalance));
	}

	private static void DrawBalance(long value)
	{
		if ((Object)(object)_balanceText != (Object)null && value != _lastDrawnBalance)
		{
			_lastDrawnBalance = value;
			((TMP_Text)_balanceText).text = $"${value:N0}";
		}
	}

	private static void ShowPopup(string message, Color color)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		_popupTimer = 3.5f;
		if (!((Object)(object)_popupText == (Object)null))
		{
			((TMP_Text)_popupText).text = message;
			((Graphic)_popupText).color = new Color(color.r, color.g, color.b, 1f);
		}
	}

	private static void AnimatePopup()
	{
		if (!((Object)(object)_popupText == (Object)null) && !(_popupTimer <= 0f))
		{
			_popupTimer -= Time.unscaledDeltaTime;
			float alpha = Mathf.Clamp01(_popupTimer / 1f);
			SetAlpha((TMP_Text)(object)_popupText, alpha);
			if (_popupTimer <= 0f)
			{
				((TMP_Text)_popupText).text = "";
			}
		}
	}

	private static void SetAlpha(TMP_Text text, float alpha)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Color color = ((Graphic)text).color;
		color.a = alpha;
		((Graphic)text).color = color;
	}

	private static void Billboard()
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (!LabRPConfig.HudBillboard.Value || (Object)(object)_rootTransform == (Object)null)
		{
			return;
		}
		Transform val = (RigData.HasPlayer ? RigData.Refs.Head : null);
		if (!((Object)(object)val == (Object)null))
		{
			Vector3 val2 = _rootTransform.position - val.position;
			if (!(((Vector3)(ref val2)).sqrMagnitude < 0.0001f))
			{
				_rootTransform.rotation = Quaternion.LookRotation(((Vector3)(ref val2)).normalized, Vector3.up);
			}
		}
	}

	private static TMP_FontAsset GetFont()
	{
		if ((Object)(object)_font != (Object)null)
		{
			return _font;
		}
		try
		{
			Il2CppReferenceArray<Object> obj = Resources.FindObjectsOfTypeAll(Il2CppType.Of<TMP_FontAsset>());
			TMP_FontAsset val = null;
			foreach (Object item in (Il2CppArrayBase<Object>)(object)obj)
			{
				TMP_FontAsset val2 = ((Il2CppObjectBase)item).TryCast<TMP_FontAsset>();
				if (!((Object)(object)val2 == (Object)null))
				{
					if (val == null)
					{
						val = val2;
					}
					if (((Object)val2).name.IndexOf("arlon-medium", StringComparison.OrdinalIgnoreCase) >= 0)
					{
						_font = val2;
						return _font;
					}
				}
			}
			_font = val;
			if ((Object)(object)_font == (Object)null)
			{
				MelonLogger.Warning("[LabRP] No TMP font assets are loaded yet; HUD text may be invisible until the next rebuild.");
			}
		}
		catch (Exception ex)
		{
			MelonLogger.Warning("[LabRP] Could not find a TMP font, HUD text may be invisible: " + ex.Message);
		}
		return _font;
	}
}
