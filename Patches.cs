using System;
using System.IO;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace SunkenCompass;

[HarmonyPatch(typeof(UICombat), nameof(UICombat.Start))]
static class UICombatStartPatch
{
    static void Prefix(UICombat __instance)
    {
        if (SunkenCompassPlugin.CompassSprite.texture.width < 1)
        {
            SunkenCompassPlugin.SunkenCompassLogger.LogDebug("Image for compass was invalid or zero pixels in width.");
        }
        else
        {
            float num = SunkenCompassPlugin.CompassSprite.texture.width / 2f;
            Sprite sprite1 = Sprite.Create(SunkenCompassPlugin.CompassSprite.texture, new Rect(0.0f, 0.0f, SunkenCompassPlugin.CompassSprite.texture.width, SunkenCompassPlugin.CompassSprite.texture.height), Vector2.zero);
            Sprite? sprite2 = null;
            if (SunkenCompassPlugin.GotCompassMask && SunkenCompassPlugin.CompassMask.texture.width > 0)
                sprite2 = Sprite.Create(SunkenCompassPlugin.CompassMask.texture, new Rect(0.0f, 0.0f, num, SunkenCompassPlugin.CompassMask.texture.height), Vector2.zero);
            Sprite? sprite3 = null;
            if (SunkenCompassPlugin.CompassShowCenterMark.Value == SunkenCompassPlugin.Toggle.On && SunkenCompassPlugin.GotCompassImage && SunkenCompassPlugin.GotCompassMask && SunkenCompassPlugin.GotCompassCenter && SunkenCompassPlugin.CompassCenter.texture.width > 0)
                sprite3 = Sprite.Create(SunkenCompassPlugin.CompassCenter.texture, new Rect(0.0f, 0.0f, SunkenCompassPlugin.CompassCenter.texture.width, SunkenCompassPlugin.CompassCenter.texture.height), Vector2.zero);
            SunkenCompassPlugin.ObjectParent = new GameObject();
            SunkenCompassPlugin.ObjectParent.name = "Compass";
            SunkenCompassPlugin.ObjectParent.AddComponent<RectTransform>().SetParent(__instance.CombatRoot.transform);


            GameObject gameObject3 = new GameObject();
            if (SunkenCompassPlugin.CompassMask.texture != null && SunkenCompassPlugin.CompassMask.texture.width > 0)
            {
                gameObject3.name = "Mask";
                RectTransform rectTransform = gameObject3.AddComponent<RectTransform>();
                rectTransform.SetParent(SunkenCompassPlugin.ObjectParent.transform);
                rectTransform.sizeDelta = new Vector2(num, SunkenCompassPlugin.CompassSprite.texture.height);
                rectTransform.localScale = Vector3.one * SunkenCompassPlugin.CompassScale.Value;
                rectTransform.anchoredPosition = Vector2.zero;
                Image image = gameObject3.AddComponent<Image>();
                image.sprite = sprite2;
                image.preserveAspect = true;
                gameObject3.AddComponent<Mask>().showMaskGraphic = false;
            }

            SunkenCompassPlugin.ObjectCompass = new GameObject();
            SunkenCompassPlugin.ObjectCompass.name = "Image";
            RectTransform rectTransform1 = SunkenCompassPlugin.ObjectCompass.AddComponent<RectTransform>();
            rectTransform1.SetParent(gameObject3.transform);
            rectTransform1.localScale = Vector3.one;
            rectTransform1.anchoredPosition = Vector2.zero;
            rectTransform1.sizeDelta = new Vector2(SunkenCompassPlugin.CompassSprite.texture.width, SunkenCompassPlugin.CompassSprite.texture.height);
            Image image1 = SunkenCompassPlugin.ObjectCompass.AddComponent<Image>();
            image1.sprite = sprite1;
            image1.preserveAspect = true;
            if (SunkenCompassPlugin.CompassShowCenterMark.Value == SunkenCompassPlugin.Toggle.On && SunkenCompassPlugin.CompassCenter.texture.width > 0)
            {
                SunkenCompassPlugin.ObjectCenterMark = new GameObject();
                SunkenCompassPlugin.ObjectCenterMark.name = "CenterMark";
                RectTransform rectTransform2 = SunkenCompassPlugin.ObjectCenterMark.AddComponent<RectTransform>();
                rectTransform2.SetParent(gameObject3.transform);
                rectTransform2.localScale = Vector3.one;
                rectTransform2.anchoredPosition = Vector2.zero;
                rectTransform2.sizeDelta = new Vector2(SunkenCompassPlugin.CompassCenter.texture.width, SunkenCompassPlugin.CompassCenter.texture.height);
                Image image2 = SunkenCompassPlugin.ObjectCenterMark.AddComponent<Image>();
                image2.sprite = sprite3;
                image2.preserveAspect = true;
            }

            SunkenCompassPlugin.SunkenCompassLogger.LogDebug("Finished attempting to add compass to game hud.");
        }
    }
}

[HarmonyPatch(typeof(UICombat), nameof(UICombat.Update))]
static class UICombatUpdatePatch
{
    static void Prefix(UICombat __instance)
    {
        if (!WorldScene.code || !Global.code.Player) return;

        if (SunkenCompassPlugin.ConfigEnabled.Value == SunkenCompassPlugin.Toggle.Off || !Global.code.Player || !SunkenCompassPlugin.GotCompassImage || !SunkenCompassPlugin.GotCompassMask)
            return;
        float num1 = SunkenCompassPlugin.CompassUsePlayerDirection.Value == SunkenCompassPlugin.Toggle.Off ? FPSPlayer.code.CameraControlComponent.transform.eulerAngles.y : Global.code.Player.transform.eulerAngles.y;
        if (num1 > 180.0)
            num1 -= 360f;
        float num2 = num1 * (-1f * (float)Math.PI / 180f);
        Rect rect = SunkenCompassPlugin.ObjectCompass.GetComponent<Image>().sprite.rect;
        SunkenCompassPlugin.ObjectCompass.GetComponent<RectTransform>().localPosition = Vector3.right * (rect.width / 2f) * num2 / 6.283185f - new Vector3(rect.width * 0.125f, 0.0f, 0.0f);
        SunkenCompassPlugin.ObjectCompass.GetComponent<Image>().color = SunkenCompassPlugin.ColorCompass.Value;
        SunkenCompassPlugin.ObjectParent.GetComponent<RectTransform>().localScale = Vector3.one * SunkenCompassPlugin.CompassScale.Value;
        SunkenCompassPlugin.ObjectParent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, (float)((Screen.height / (double)1 - SunkenCompassPlugin.ObjectCompass.GetComponent<Image>().sprite.texture.height * (double)SunkenCompassPlugin.CompassScale.Value) / 2.0)) - Vector2.up * SunkenCompassPlugin.CompassYOffset.Value;

        if (SunkenCompassPlugin.CompassShowCenterMark.Value != SunkenCompassPlugin.Toggle.On || SunkenCompassPlugin.ObjectCenterMark == null) return;
        SunkenCompassPlugin.ObjectCenterMark.GetComponent<Image>().color = SunkenCompassPlugin.ColorCenterMark.Value;
        SunkenCompassPlugin.ObjectCenterMark.SetActive(true);
    }
}