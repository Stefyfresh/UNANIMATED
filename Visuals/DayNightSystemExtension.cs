using System.Collections;
using Overworld;
using UNANIMATED.Visuals;
using UnityEngine;

public static class DayNightSystemExtension
{
    public static void SetWeatherOverrideLerp(this DayNightSystem dayNight, string _weather, string _prevWeather, float duration)
    {
        if (dayNight.gameObject.activeInHierarchy) dayNight.StartCoroutine(SetWeatherOverrideLerpCoroutine(dayNight, _weather, _prevWeather, duration));
        else dayNight.SetWeatherOverride(_weather);
    }


    // Modified version of the existing weather lerp coroutine
    private static IEnumerator SetWeatherOverrideLerpCoroutine(DayNightSystem dayNight, string _weather, string _prevWeather, float duration)
    {
        Light light = dayNight.GetComponent<Light>();

        dayNight.RebuildNamedWeather();
        Weather newWeather = dayNight.namedWeather[_weather];
        Weather prevWeather = dayNight.namedWeather[_prevWeather];

        for (float anim = 0; anim < duration; anim += Time.deltaTime)
        {
            try
            {
                float progress = anim / duration;
                light.color = Color.Lerp(prevWeather.color, newWeather.color, progress);
                light.intensity = Mathf.Lerp(prevWeather.intensity, newWeather.intensity, progress);
                light.transform.localRotation = Quaternion.Lerp(Quaternion.Euler(prevWeather.orientation), Quaternion.Euler(newWeather.orientation), progress);
                RenderSettings.sun = light;
                RenderSettings.skybox = (progress > 0.5f) ? newWeather.skybox : prevWeather.skybox;
                RenderSettings.ambientLight = Color.Lerp(prevWeather.bounceLight, newWeather.bounceLight, progress);
                RenderSettings.fog = true;
                RenderSettings.fogMode = FogMode.ExponentialSquared;
                RenderSettings.fogColor = Color.Lerp(prevWeather.fogColor, newWeather.fogColor, progress);
                RenderSettings.fogDensity = Mathf.Lerp(prevWeather.fogDensity, newWeather.fogDensity, progress);
                RenderSettings.skybox.SetFloat("_CloudRotation", light.transform.eulerAngles.y);
            }
            catch { }
            yield return null;
        }
    }


    // Modified version of SetWeatherOverride that allows directly passing in a Weather
    public static void SetWeatherOverride(this DayNightSystem dayNight, Weather weather)
    {
        try
        {
            Light component = dayNight.GetComponent<Light>();
            component.color = weather.color;
            component.intensity = weather.intensity;
            component.transform.eulerAngles = weather.orientation;
            RenderSettings.sun = component;
            RenderSettings.skybox = weather.skybox;
            RenderSettings.ambientLight = weather.bounceLight;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = weather.fogColor;
            RenderSettings.fogDensity = weather.fogDensity;
            weather.skybox.SetFloat("_CloudRotation", component.transform.eulerAngles.y);
        }
        catch { }
    }
}