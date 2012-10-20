<?php

include("functions.php");
include("classes.php");

if(!isLoggedIn()) {
  echo "Not logged in.";
  exit;
}

$roomID = $_POST['r'];
if(strstr($roomID, "/") || strstr($roomID, "?")) {
  echo "";
  exit;
}

$message = $_POST['m'];

$me = me();
echo file_get_contents($_SETTINGS['RTS_IntURL'] . "c/" . $roomID . "/" . $me->username . "?" . urlencode($message));

?>