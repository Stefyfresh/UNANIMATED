using System.Collections;
using Overworld;
using UNANIMATED.Visuals;
using UnityEngine;

public static class DayNightSystemExtension
{
    public static Coroutine weatherCoroutine;
    public static Coroutine cloudRotationCoroutine;

    public static void SetWeatherOverrideLerp(this DayNightSystem dayNight, string _weather, string _prevWeather, float duration, Light sun)
    {
        if (dayNight.gameObject.activeInHierarchy) weatherCoroutine = dayNight.StartCoroutine(SetWeatherOverrideLerpCoroutine(dayNight, sun, _weather, _prevWeather, duration));
        else dayNight.SetWeatherOverride(_weather);
    }

    public static void SetCloudRotationLerp(this DayNightSystem dayNight, float startRotation, float endRotation, float duration)
    {
        if (dayNight.gameObject.activeInHierarchy) cloudRotationCoroutine = dayNight.StartCoroutine(SetCloudRotationLerpCoroutine(startRotation, endRotation, duration));
        else dayNight.SetCloudRotation(endRotation);
    }


    // Modified version of the existing weather lerp coroutine
    private static IEnumerator SetWeatherOverrideLerpCoroutine(DayNightSystem dayNight, Light sun, string _weather, string _prevWeather, float duration)
    {
        dayNight.RebuildNamedWeather();
        Weather newWeather = dayNight.namedWeather[_weather];
        Weather prevWeather = dayNight.namedWeather[_prevWeather];

        for (float anim = 0; anim < duration; anim += Time.deltaTime)
        {
            try
            {
                float progress = anim / duration;
                RenderSettings.skybox = (progress > 0.5f) ? newWeather.skybox : prevWeather.skybox;
                RenderSettings.ambientLight = Color.Lerp(prevWeather.bounceLight, newWeather.bounceLight, progress);
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogColor = Color.Lerp(prevWeather.fogColor, newWeather.fogColor, progress);
                RenderSettings.fogDensity = Mathf.Lerp(prevWeather.fogDensity, newWeather.fogDensity, progress);
                if (sun)
                {
                    sun.color = Color.Lerp(prevWeather.color, newWeather.color, progress);
                    sun.intensity = Mathf.Lerp(prevWeather.intensity, newWeather.intensity, progress);
                    if (sun == VisualController.masterSun) sun.intensity *= 0.5f;
                    sun.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(prevWeather.orientation), Quaternion.Euler(newWeather.orientation), progress);
                    RenderSettings.sun = sun;
                    VisualController.currentCloudRotation = sun.transform.eulerAngles.y;
                    RenderSettings.skybox.SetFloat("_CloudRotation", VisualController.currentCloudRotation);
                }
            }
            catch { }
            yield return null;
        }
    }


    private static IEnumerator SetCloudRotationLerpCoroutine(float startRotation, float endRotation, float duration)
    {
        for (float anim = 0; anim < duration; anim += Time.deltaTime)
        {
            try
            {
                float progress = anim / duration;
                RenderSettings.skybox.SetFloat("_CloudRotation", Mathf.Lerp(startRotation, endRotation, progress));
            }
            catch { }
            yield return null;
        }
        yield return null;
    }


    // Modified version of SetWeatherOverride that allows directly passing in a Weather
    public static void SetWeatherOverride(this DayNightSystem dayNight, Weather weather, Light sun)
    {
        try
        {
            RenderSettings.skybox = weather.skybox;
            RenderSettings.ambientLight = weather.bounceLight;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = weather.fogColor;
            RenderSettings.fogDensity = weather.fogDensity;
            if (sun)
            {
                sun.color = weather.color;
                sun.intensity = weather.intensity;
                if (sun == VisualController.masterSun) sun.intensity *= 0.5f;
                sun.transform.eulerAngles = weather.orientation;
                RenderSettings.sun = sun;
                VisualController.currentCloudRotation = sun.transform.eulerAngles.y;
                RenderSettings.skybox.SetFloat("_CloudRotation", VisualController.currentCloudRotation);
            }
        }
        catch { }
    }


    // Modified version of SetWeatherOverride that allows directly passing in a Light
    public static void SetWeatherOverride(this DayNightSystem dayNight, string _weather, Light sun)
    {
        dayNight.RebuildNamedWeather();
        try
        {
            Weather weather = dayNight.namedWeather[_weather];
            RenderSettings.skybox = weather.skybox;
            RenderSettings.ambientLight = weather.bounceLight;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = weather.fogColor;
            RenderSettings.fogDensity = weather.fogDensity;
            if (sun)
            {
                sun.color = weather.color;
                sun.intensity = weather.intensity;
                if (sun == VisualController.masterSun) sun.intensity *= 0.5f;
                sun.transform.eulerAngles = weather.orientation;
                RenderSettings.sun = sun;
                VisualController.currentCloudRotation = sun.transform.eulerAngles.y;
                RenderSettings.skybox.SetFloat("_CloudRotation", VisualController.currentCloudRotation);
            }
        }
        catch { }
    }


    public static void SetCloudRotation(this DayNightSystem dayNight, float rotation)
    {
        try
        {
            RenderSettings.skybox.SetFloat("_CloudRotation", rotation);
        }
        catch { }
    }
}