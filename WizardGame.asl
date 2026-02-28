state("WizardGame")
{
}

startup
{
    refreshRate = 60;
}

init
{
    vars.accessor = null;
    vars.ResetPrevState = (Action)(() => {
        vars.prev = new ExpandoObject();
		vars.prev.wave1started  = false;
        vars.prev.wave1finished = false;
		vars.prev.wave2started  = false;
		vars.prev.wave2finished = false;
		vars.prev.wave3started  = false;
        vars.prev.wave3finished = false;
        vars.prev.wave4started  = false;
        vars.prev.wave4finished = false;
        vars.prev.wave5started  = false;
		vars.prev.wave5finished = false;
		vars.prev.hubOpened     = false;

        vars.prev.gameFinished  = false;
    });
    vars.ResetPrevState();
}

update
{
    if (vars.accessor == null)
    {
        try
        {
            var mmf = System.IO.MemoryMappedFiles.MemoryMappedFile.OpenExisting("WizardGameSpeedrun");
            vars.accessor = mmf.CreateViewAccessor();
        }
        catch { return false; }
    }
	
    vars.isRunStarted  = vars.accessor.ReadBoolean(0);
	vars.wave1started  = vars.accessor.ReadBoolean(1);
	vars.wave1finished = vars.accessor.ReadBoolean(2);
	vars.wave2started  = vars.accessor.ReadBoolean(3);
	vars.wave2finished = vars.accessor.ReadBoolean(4);
	vars.wave3started  = vars.accessor.ReadBoolean(5);
	vars.wave3finished = vars.accessor.ReadBoolean(6);
	vars.wave4started  = vars.accessor.ReadBoolean(7);
	vars.wave4finished = vars.accessor.ReadBoolean(8);
	vars.wave5started  = vars.accessor.ReadBoolean(9);
	vars.wave5finished = vars.accessor.ReadBoolean(10);
	vars.hubOpened     = vars.accessor.ReadBoolean(11);
	vars.gameFinished  = vars.accessor.ReadBoolean(12);
}

start
{
    return vars.isRunStarted;
}

split
{
    if (vars.wave1finished && !vars.prev.wave1finished) { vars.prev.wave1finished = true; return true; }
	if (vars.wave1started && !vars.prev.wave1started) { vars.prev.wave1started = true; return true; }
    if (vars.wave2finished && !vars.prev.wave2finished) { vars.prev.wave2finished = true; return true; }
	if (vars.wave2started && !vars.prev.wave2started) { vars.prev.wave2started = true; return true; }
    if (vars.wave3finished && !vars.prev.wave3finished) { vars.prev.wave3finished = true; return true; }
	if (vars.wave3started && !vars.prev.wave3started) { vars.prev.wave3started = true; return true; }
	if (vars.wave4finished && !vars.prev.wave4finished) { vars.prev.wave4finished = true; return true; }
	if (vars.wave4started && !vars.prev.wave4started) { vars.prev.wave4started = true; return true; }
	if (vars.wave5finished && !vars.prev.wave5finished) { vars.prev.wave5finished = true; return true; }
	if (vars.wave5started && !vars.prev.wave5started) { vars.prev.wave5started = true; return true; }
	if (vars.hubOpened && !vars.prev.hubOpened) { vars.prev.hubOpened = true; return true; }
    if (vars.gameFinished  && !vars.prev.gameFinished)  { vars.prev.gameFinished  = true; return true; }
}

reset
{
    // Reset prevState when a new run starts so splits fire again
    if (!vars.isRunStarted){
        vars.ResetPrevState();
		return true;
    }
    return false;
}