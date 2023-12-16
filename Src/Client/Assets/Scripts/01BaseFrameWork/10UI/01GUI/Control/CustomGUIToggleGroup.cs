using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGUIToggleGroup : MonoBehaviour
{
    public CustomGUIToggle[] toggles;
    private CustomGUIToggle frontTurTog;

    void Start()
    {
        if (toggles.Length == 0)
            return;

        for (int i = 0; i < toggles.Length; i++)
        {
            CustomGUIToggle toggle = toggles[i];
            toggle.changeValue += (value) =>
            {
                if( value )
                {
                    for (int j = 0; j < toggles.Length; j++)
                    {
                        if( toggles[j] != toggle )
                        {
                            toggles[j].isSel = false;
                        }
                    }
                    frontTurTog = toggle;
                }
                else if( toggle == frontTurTog)
                {
                    toggle.isSel = true;
                }
            };
        }
    }

}
