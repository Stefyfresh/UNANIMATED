using System.Collections;
using Overworld;
using UNANIMATED.Visuals;
using UnityEngine;

public static class DayNightSystemExtension
{
    public static void SetWeatherOverrideLerp(this DayNightSystem dayNight, string _weather, string _prevWeather, float duration)
    {
        dayNight.StartCoroutine(SetWeatherOverrideLerpCoroutine(dayNight, _weather, _prevWeather, duration));
    }


    // Modified version of the existing weather lerp coroutine
    public static IEnumerator SetWeatherOverrideLerpCoroutine(DayNightSystem dayNight, string _weather, string _prevWeather, float duration)
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
}