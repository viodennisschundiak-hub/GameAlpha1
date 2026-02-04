using Bobo.Data;
using Bobo.Simulation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Bobo.UI
{
    public sealed class WorldCreatorController : MonoBehaviour
    {
        [SerializeField] private InputField worldNameInput;
        [SerializeField] private InputField seedInput;
        [SerializeField] private Dropdown presetDropdown;
        [SerializeField] private Slider densitySlider;
        [SerializeField] private SimulationSettings settings;

        public void CreateWorld()
        {
            WorldCreationData.WorldName = worldNameInput != null ? worldNameInput.text : "New World";
            WorldCreationData.Seed = seedInput != null && int.TryParse(seedInput.text, out int seed) ? seed : 0;
            WorldCreationData.Preset = presetDropdown != null ? presetDropdown.options[presetDropdown.value].text : "Random";
            WorldCreationData.StartDensity = densitySlider != null ? densitySlider.value : 0.2f;
            WorldCreationData.Settings = settings;
            SceneManager.LoadScene("GameScene");
        }
    }
}
