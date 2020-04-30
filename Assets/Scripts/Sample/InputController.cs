using Base.PersistManager;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Sample
{
    public class InputController : MonoBehaviour
    {

#pragma warning disable 649
        [SerializeField] private InputField _input;
        [SerializeField] private Text _output;

        [Inject] private readonly IPersistManager _persistentManager;
#pragma warning restore 649

        public void OnSave()
        {
            PersistentObject ob = new PersistentObject();
            ob._data = _input.text;
            _input.text = "";
            _persistentManager.Persist(ob);

        }
        
        public void OnLoad()
        {
            PersistentObject ob = new PersistentObject();
            _persistentManager.Restore(ob);
            _output.text = ob._data;
        }
    }
}