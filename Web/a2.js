var $ = function(selector) {
	if(selector == undefined) {
		return this;
	}else{
		switch(typeof(selector)) {
			case "string":
				return $.select(selector);
			case "object":
				var ret = [];
				for(o in selector) {
					ret[o] = $.select(o);
				}
				return ret;
				break;
		}
	}
};
$.select = function(selector) {
	var origin = document;
	if(this != $) {
		origin = this;
	}
	
	var ret = null;
	switch(selector[0]) {
		case "#":
			ret = document.getElementById(selector.substr(1));
			break;
		case ".":
			ret = origin.getElementsByClassName(selector.substr(1));
			break;
		default:
			ret = origin.getElementsByTagName(selector);
			break;
	}
	
	if(ret == null) return null;
	
	if(ret.length != undefined) {
		if(ret.length == 1) {
			return this.addFuncs(ret[0]);
		}
	}
	
	return ret;
};
$.ready = function(callback) {
	document.addEventListener("DOMContentLoaded", callback, false);
};
$.addFuncs = function(obj) {
	var ret = obj;
	
	// Content
	ret.set = function(key, str) {
		if(str == undefined) {
			ret.innerHTML = key;
			return ret;
		}else{
			ret[key] = str;
			return ret;
		}
	}
	ret.get = function() { return ret.innerHTML; }
	ret.set = function(str) { ret.innerHTML = str; return ret; }
	ret.append = function(str) { ret.innerHTML += str; return ret; }
	ret.prepend = function(str) { ret.innerHTML = ret.innerHTML + str; return ret; }
	ret.empty = function() { ret.innerHTML = ""; return ret; }
	ret.remove = function() { ret.parentNode.removeChild(ret); }
	ret.setClass = function(str) { ret.className = str; return ret; }
	ret.setId = function(str) { ret.id = str; return ret; }
	
	ret = this.addAliases(ret);
	
	for(o in $) ret[o] = $[o];
	return ret;
};
$.addAliases = function(obj) {
	var ret = obj;
	
	if(ret.parentNode != null) ret.parent = this.addFuncs(ret.parentNode);
	
	return ret;
};
$.create = function(elem, innerHtml) {
	if(elem == undefined) return null;
	
	var ret = document.createElement(elem);
	if(innerHtml != undefined) ret.innerHTML = innerHtml;
	
	return this.addFuncs(ret);
};
$.make = function(elem, innerHtml, addTo) {
	if(elem == undefined) return null;
	
	var ret = $.create(elem, innerHtml);
	
	if(addTo != undefined) {
		addTo.appendChild(ret);
	}else{
		if(this != $) {
			this.appendChild(ret);
		}else{
			document.body.appendChild(ret);
		}
	}
	
	return this.addAliases(ret);
};
$.get = function(url, callback) {
	var req;
	if(window.XMLHttpRequest)
		req = new XMLHttpRequest();
	else
		req = new ActiveXObject("Microsoft.XMLHTTP");
	
	if(req) {
		req.onreadystatechange = function() {
			if(req.readyState == 4 && req.status == 200)
				callback(req.responseText);
		};
		
		req.open("GET", url, true);
		req.send();
	}
};
$.post = function(url, data, callback) {
	var req;
	if(window.XMLHttpRequest)
		req = new XMLHttpRequest();
	else
		req = new ActiveXObject("Microsoft.XMLHTTP");
	
	if(req) {
		req.onreadystatechange = function() {
			if(req.readyState == 4 && req.status == 200)
				callback(req.responseText);
		};
		
		req.open("POST", url, true);
		req.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
		req.setRequestHeader("Content-Length", data.length);
		req.setRequestHeader("Connection", "close");
		req.send(data);
	}
};