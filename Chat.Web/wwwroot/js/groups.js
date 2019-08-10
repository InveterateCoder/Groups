var app;
document.addEventListener('DOMContentLoaded', () => {
    app = new app_class();
    window.addEventListener('resize', () => {
        app.on_resize();
    });
});
class api_class {
    constructor() { }
    async register(chatterer) {
        try {
            let ret = false;
            let resp = await this.post('/api/account/reg', chatterer);
            switch (resp){
                case "registered":
                    app.alert("already registered");
                    break;
                case "email_is_taken":
                    app.alert("this email is registered already")
                    break;
                case "name_is_taken":
                    app.alert("this nick name has been taken by another user")
                    break;
                case "pending_" + chatterer.email:
                    ret = true;
                    break;
                default:
                    app.alert(resp);
                    break;
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async validate(number) {
        try {
            let ret = false;
            let resp = await this.post('api/account/val', number);
            if (resp.startsWith('added_'))
                ret = true;
            else if (resp == 'registered')
                app.alert('already registered');
            else if (resp == 'invalid_confirmation_id')
                app.alert('wrong number');
            else if (resp == 'register_request_required')
                app.alert('registration is required prior to email verification');
            else if (resp == 'email_is_taken')
                app.alert('this email is registered already');
            else if (resp == 'name_is_taken')
                app.alert("this nickname has been already taken by another user");
            else app.alert(resp);
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async signin(chatterer) {
        try {
            let ret = false;
            let resp = await this.post('api/account/sign', chatterer);
            switch (resp) {
                case 'already_signed':
                    document.cookie = 'Chat_Authentication_Token=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/;';
                    app.alert('signed out, try again');
                    break;
                case 'user_not_found':
                    app.alert("email address was not found");
                    break;
                case 'password_incorrect':
                    app.alert('wrong password, try again')
                    break;
                case 'multiple_signins_forbidden':
                    app.alert('access denied, already signed in');
                    break;
                case 'OK':
                    ret = true;
                    break;
                default:
                    app.alert(resp);
            }
            return ret;
        }
        catch (err) {
            app.alert(err.message);
            return false;
        }
    }
    async post(addr, obj) {
        app.wait();
        let ret;
        try {
            let resp = await fetch(addr, {
                method: 'post',
                headers: {
                    'Accept': 'plain/text', 'Content-Type': 'application/json'
                },
                body: JSON.stringify(obj)
            });
            ret = await resp.text();
        }
        catch (err) {
            ret = err.message;
        }
        app.resume();
        return ret;
    }
}
class reg_panel_class {
    constructor() {
        this.panel = document.getElementById('reg_panel');
        this.place = 'home';
        this.move();
    }
    move() {
        let mt = (window.innerHeight - this.panel.offsetHeight) / 2;
        mt = mt < 0 ? 0 : mt;
        this.panel.style.marginTop = mt + 'px';
    }
    num(e) {
        if (isNaN(e.key))
            e.preventDefault();
    }
    pasteFilter(e) {
        if (isNaN(e.target.value))
            e.target.value = null;
    }
    clear() {
        let arr;
        switch (this.place) {
            case 'reg':
                arr = this.panel.children[2].getElementsByTagName('input');
                for (let i = 0; i < arr.length; i++)
                    arr[i].value = null;
                break;
            case 'sign':
                arr = this.panel.children[3].getElementsByTagName('input');
                for (let i = 0; i < arr.length; i++)
                    arr[i].value = null;
                break;
            case 'numb':
                this.panel.children[4].getElementsByTagName('input')[0].value = null;
                break;
        }
    }
    btn_hover(el) {
        el.firstElementChild.style.transform = "scale(1.1,1.1)";
    }
    btn_out(el) {
        el.firstElementChild.style.transform = "scale(1,1)";
    }
    btn_down(el) {
        el.firstElementChild.style.transform = "scale(0.9,0.9)";
    }
    btn_up(el) {
        el.firstElementChild.style.transform = "scale(1.1,1.1)";
    }
    goto(place) {
        switch (place) {
            case 'home':
                this.hide();
                this.panel.children[1].style.display = 'flex';
                break;
            case 'reg':
                this.hide();
                this.panel.children[2].style.display = 'flex';
                break;
            case 'sign':
                this.hide();
                this.panel.children[3].style.display = 'flex';
                break;
            case 'numb':
                this.hide();
                this.panel.children[4].style.display = 'flex';
                break;
        }
        this.place = place;
    }
    hide() {
        switch (this.place) {
            case 'home':
                this.panel.children[1].style.display = 'none';
                break;
            case 'reg':
                this.clear();
                this.panel.children[2].style.display = 'none';
                break;
            case 'sign':
                this.clear();
                this.panel.children[3].style.display = 'none';
                break;
            case 'numb':
                this.clear();
                this.panel.children[4].style.display = 'none';
                break;
        }
    }
    register() {
        let form = this.panel.children[2].getElementsByTagName('input');
        let chatterer = {
            name: form[0].value,
            email: form[1].value,
            password: form[2].value
        };
        if (chatterer.name.length < 5 || chatterer.name.length > 64)
            app.alert("the name's length should be minimum 5 characters and maximum 64");
        else if (chatterer.email.length == 0 || !form[1].checkValidity())
            app.alert('wrong email address');
        else if (chatterer.password.length < 8 || chatterer.password.length > 32)
            app.alert("the password's length should be minimum 8 characters and maximum 32");
        else {
            app.api.register(chatterer).then(ok => {
                if (ok)
                    this.goto("numb");
            });
        }
    }
    validate() {
        let num = this.panel.children[4].getElementsByTagName('input')[0].value;
        if (num.length != 4)
            app.alert("code must consist of 4 digits");
        else {
            app.api.validate(Number(num)).then(ok => {
                if (ok) {
                    this.goto("sign");
                }
            });
        }
    }
    signin() {
        let form = this.panel.children[3].getElementsByTagName('input');
        let chatterer = {
            email: form[0].value,
            password: form[1].value
        };
        if (chatterer.email.length == 0 || !form[0].checkValidity())
            app.alert("wrong email address");
        else if (chatterer.password.length < 8 || chatterer.password.length > 32)
            app.alert("the password's length should be minimum 8 characters and maximum 32");
        //todo implement signing
    }
}
class app_class {
    constructor() {
        this.api = new api_class();
        this.reg_panel = new reg_panel_class();
    }
    on_resize() {
        this.reg_panel.move();
    }
    wait() {
        document.getElementById('wait').style.visibility = 'visible';
    }
    resume() {
        document.getElementById('wait').style.visibility = 'hidden';
    }
    alert(message) {
        let el = document.getElementById('message');
        el.children[0].children[0].textContent = message;
        el.style.display = 'block';
    }
}