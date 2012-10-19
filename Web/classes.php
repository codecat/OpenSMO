<?php
class User
{
	public $id = 0;
	public $username = "";
	public $password = "";
	public $email = "";
	public $rank = 0;
	public $xp = 0;
	
	function __construct($user)
	{
		if(!is_array($user))
		{
			if(is_string($user))
				$res = sqlQuery("SELECT * FROM \"users\" WHERE \"Username\"='".makeSafeSQL($user)."'");
			elseif(is_int($user))
				$res = sqlQuery("SELECT * FROM \"users\" WHERE \"ID\"=".intval($user));
			
			if(count($res) == 0 || !isset($res))
				return;
			
			$row = $res[0];
		}else
			$row = $user;
		
		if(!isset($row))
			return;
		
		$this->id = $row['ID'];
		$this->username = $row['Username'];
		$this->password = $row['Password'];
		$this->email = $row['Email'];
		$this->rank = $row['Rank'];
		$this->xp = $row['XP'];
	}
	
	function linkName()
	{
		return "<a class=\"userlink rank".$this->rank."\" href=\"index.php?page=profile&uid=".$this->id."\">".makeSafeHTML($this->username)."</a>";
	}
	
	function level()
	{
		//TODO: Better calculation...
		return floor(sqrt($this->xp / 750));
	}
	
	function save()
	{
		if($this->id == 0 || !is_int($this->id))
			return;
		
		sqlQuery("UPDATE \"users\" SET \"Username\"='".makeSafeSQL($this->username)."', \"Password\"='".$this->password."', \"Rank\"=".intval($this->rank)." WHERE \"ID\"=".$this->id);
	}
}

class Song
{
	public $id = 0;
	public $name = "";
	public $artist = "";
	public $subtitle = "";
	public $played = 0;
	
	function __construct($song)
	{
		if(!is_array($song))
		{
			if(is_int($song))
				$res = sqlQuery("SELECT * FROM \"songs\" WHERE \"ID\"=".intval($song));
			
			if(count($res) == 0 || !isset($res))
				return;
			
			$row = $res[0];
		}else
			$row = $song;
		
		if(!isset($row))
			return;
		
		$this->id = $row['ID'];
		$this->name = $row['Name'];
		$this->artist = $row['Artist'];
		$this->subtitle = $row['SubTitle'];
		$this->played = $row['Played'];
	}
	
	function linkName()
	{
		return "<a href=\"index.php?page=song&sid=".$this->id."\">".makeSafeHTML($this->name)."</a>";
	}
}

class Stat
{
	public $id = 0;
	public $user = false;
	public $playersettings = "";
	public $song = false;
	public $feet = 0;
	public $difficulty = 0;
	public $grade = 0;
	public $score = 0;
	public $maxcombo = 0;
	public $notes = array();
	
	private $row;
	
	function __construct($stat)
	{
		if(!is_array($stat))
		{
			if(is_int($stat))
				$res = sqlQuery("SELECT * FROM \"stats\" WHERE \"ID\"=".intval($stat));
			
			if(count($res) == 0 || !isset($res))
				return;
			
			$row = $res[0];
		}else
			$row = $stat;
		
		if(!isset($row))
			return;
		
		$this->id = $row['ID'];
		$this->playersettings = $row['PlayerSettings'];
		$this->feet = $row['Feet'];
		$this->difficulty = $row['Difficulty'];
		$this->grade = $row['Grade'];
		$this->score = $row['Score'];
		$this->maxcombo = $row['MaxCombo'];
		$this->notes[0] = $row['Note_0'];
		$this->notes[1] = $row['Note_1'];
		$this->notes[2] = $row['Note_Mine'];
		$this->notes[3] = $row['Note_Miss'];
		$this->notes[4] = $row['Note_Barely'];
		$this->notes[5] = $row['Note_Good'];
		$this->notes[6] = $row['Note_Great'];
		$this->notes[7] = $row['Note_Perfect'];
		$this->notes[8] = $row['Note_Flawless'];
		$this->notes[9] = $row['Note_NG'];
		$this->notes[10] = $row['Note_Held'];
		
		$this->row = $row;
	}
	
	function fetchUser()
	{
		if(!$this->user)
			$this->user = new User(intval($this->row['User']));
	}
	
	function fetchSong()
	{
		if(!$this->song)
			$this->song = new Song(intval($this->row['Song']));
	}
	
	function echoGradeImage()
	{
		echo "<img src=\"images/grade_".getGrade($this->grade).".png\" />";
	}
}
?>